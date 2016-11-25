//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace Aos.Messaging
{
    /// <summary>
    /// Class used for creating pipe communication servers.
    /// The PipeConnected event is used to retrieve messengers when a client connects.
    /// </summary>
    public sealed class PipeCommunicatorServer : IChannelConnectNotifier, IDisposable
    {
        private readonly string pipeName;
        private readonly PipeSecurity pipeSecurity;
        private readonly ConcurrentDictionary<NamedPipeServerStream, bool> pipeServers = new ConcurrentDictionary<NamedPipeServerStream, bool>();

        public PipeCommunicatorServer(string pipeName) : this(pipeName, null)
        {
        }

        public PipeCommunicatorServer(string pipeName, PipeSecurity pipeSecurity)
        {
            this.pipeSecurity = pipeSecurity;
            this.pipeName = pipeName;
            PipeConnected += (sender, args) => { };
        }

        public event EventHandler<PipeConnectionArgs> PipeConnected;
        public event EventHandler<PipeConnectionArgs> PipeDisconnected;

        public IAuthorizationChecker AuthorizationChecker { get; set; }

        private static NamedPipeClientStream CreateOutputStream(NamedPipeServerStream srv)
        {
            var stream = new MessageStream(srv);
            var outPipeName = stream.ReadMessage();
            var outStream = new NamedPipeClientStream(".", outPipeName, PipeDirection.Out);
            outStream.Connect(2000);
            return outStream;
        }

        public void Start()
        {
            CreateListener();
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            var srv = (NamedPipeServerStream)asyncResult.AsyncState;
            try
            {
                srv.EndWaitForConnection(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            CreateListener();

            if (!CheckPipeAuthorization(srv))
            {
                Log.Warning("Disconnecting unauthorized client.");
                srv.Close();
                srv.Dispose();
                return;
            }

            PipeMessenger messenger = null;
            try
            {
                var outStream = CreateOutputStream(srv);
                messenger = new PipeMessenger(srv, outStream);

                PipeConnected?.Invoke(this, new PipeConnectionArgs(messenger));

                messenger.RunReadMessageLoop();

                PipeDisconnected?.Invoke(this, new PipeConnectionArgs(messenger));
                Log.Information("Named pipe client disconnected.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex);
            }

            messenger?.Dispose();
            bool dummy;
            this.pipeServers.TryRemove(srv, out dummy);
        }

        private bool CheckPipeAuthorization(NamedPipeServerStream srv)
        {
            return AuthorizationChecker == null || AuthorizationChecker.Check(srv);
        }

        private void CreateListener()
        {
            var server = this.pipeSecurity != null ? 
                new NamedPipeServerStream(this.pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, this.pipeSecurity)
                : new NamedPipeServerStream(this.pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            this.pipeServers[server] = true;

            try
            {
                Log.Information($"Named pipe server {this.pipeName}: start listening ...");
                server.BeginWaitForConnection(OnConnect, server);
            }
            catch (System.IO.IOException ex)
            {
                Log.Error("BeginWaitForConnection failed.", ex);
            }
        }

        public void Dispose()
        {
            foreach (var server in this.pipeServers.Keys)
            {
                server.Dispose();
            }
        }
    }
}
