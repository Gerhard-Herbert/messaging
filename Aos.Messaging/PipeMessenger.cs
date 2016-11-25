//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Aos.Messaging
{
    /// <summary>
    /// Messenger implementation for named pipes.
    /// Two pipes are used for duplex communication.
    /// </summary>
    /// <remarks>
    /// Unfortunately handling the communication using one pipe results in a lot of deadlocks which were pretty hard to resolve.
    /// It seems like the WCF duplex communication can handle tow-way communication with one pipe, so it should be possible :)
    /// </remarks>
    public class PipeMessenger : IMessenger
    {
        private readonly string outPipeName;
        private readonly PipeSecurity pipeSecurity;

        private MessageStream outMessageStream;
        private int connectionState = PipeMessengerState.Disconnected;

        public PipeMessenger(PipeStream inStream, PipeStream outStream)
        {
            InStream = inStream;
            OutStream = outStream;
            this.outMessageStream = new MessageStream(outStream);
        }

        public PipeMessenger(string outPipeName, int timeout = 500, PipeSecurity pipeSecurity = null)
        {
            this.pipeSecurity = pipeSecurity;
            this.outPipeName = outPipeName;
            Connect(timeout);
        }

        public event EventHandler<MessageReceivedEvent> MessageReceived;

        public event EventHandler ConnectionReestablished;

        private PipeStream InStream { get; set; }
        private PipeStream OutStream { get; set; }

        public void Send(string message)
        {
            if (this.outMessageStream == null && string.IsNullOrEmpty(this.outPipeName))
            {
                throw new MessengerClosedException();
            }

            try
            {
                EnsureConnected();
                this.outMessageStream.WriteMessage(message);
            }
            catch (ObjectDisposedException)
            {
                // Seems like the server closed the connection
                // Try once again
                EnsureConnected();
                this.outMessageStream.WriteMessage(message);
            }
        }

        public void RunReadMessageLoop()
        {
            try
            {
                var messageStream = new MessageStream(InStream);

                while (true)
                {
                    var message = messageStream.ReadMessage();
                    if (message == MessageStream.EndOfStream)
                    {
                        break;
                    }

                    ThreadPool.QueueUserWorkItem(o => HandleMessage(message));
                }
            }
            catch (System.IO.IOException ex)
            {
                Log.Error("Reading failed.", ex);
            }
            catch (ObjectDisposedException)
            {
                // Can ignore this, means the other endpoint closed the pipe
            }
        }

        public void Dispose()
        {
            Close();
        }

        private static void Connected(IAsyncResult ar)
        {
            var messenger = (PipeMessenger)ar.AsyncState;
            if (messenger?.InStream == null)
            {
                Log.Warning("PipeMessenger.Connected : PipeMessenger instance is not correct");
                return;
            }

            if (PipeMessengerState.Connected == Interlocked.CompareExchange(
                ref messenger.connectionState,
                PipeMessengerState.Connected,
                PipeMessengerState.Connecting))
            {
                // we already have one thread running here
                return;
            }

            try
            {
                ((NamedPipeServerStream)messenger.InStream).EndWaitForConnection(ar);
                messenger.RunReadMessageLoop();
            }
            catch (Exception ex)
            {
                Log.Warning(ex);
            }

            messenger.Close();

            // give chance to reestablish connection
            messenger.connectionState = PipeMessengerState.Disconnected;
        }

        private NamedPipeServerStream CreateRecievingPipeServer(Stream client)
        {
            var receivingPipe = Guid.NewGuid().ToString();
            var messageStream = new MessageStream(client);
            messageStream.WriteMessage(receivingPipe);

            return this.pipeSecurity != null
                ? new NamedPipeServerStream(receivingPipe, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, this.pipeSecurity)
                : new NamedPipeServerStream(receivingPipe, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        }

        private void Connect(int timeout = 500)
        {
            if (PipeMessengerState.Disconnected != Interlocked.CompareExchange(
                ref this.connectionState,
                PipeMessengerState.Connecting,
                PipeMessengerState.Disconnected))
            {
                // we already have one connection going on
                return;
            }

            NamedPipeClientStream outStream = null;
            NamedPipeServerStream inStream = null;

            try
            {
                outStream = new NamedPipeClientStream(".", this.outPipeName, PipeDirection.Out);
                outStream.Connect(timeout);

                inStream = CreateRecievingPipeServer(outStream);

                InStream = inStream;
                OutStream = outStream;
                this.outMessageStream = new MessageStream(outStream);

                inStream.BeginWaitForConnection(Connected, this);
            }
            catch (Exception)
            {
                outStream?.Dispose();
                inStream?.Dispose();
                Close();
                this.connectionState = PipeMessengerState.Disconnected;

                throw;
            }
        }

        private void Close()
        {
            this.outMessageStream = null;

            InStream?.Dispose();
            OutStream?.Dispose();

            InStream = null;
            OutStream = null;
        }

        private void EnsureConnected()
        {
            if (this.connectionState == PipeMessengerState.Disconnected && !string.IsNullOrEmpty(this.outPipeName))
            {
                Connect();
                ConnectionReestablished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                MessageReceived?.Invoke(this, new MessageReceivedEvent { Message = message });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to handle message {0}", message);
                Log.Error(ex);
            }
        }

        private sealed class PipeMessengerState
        {
            public const int Disconnected = 0;
            public const int Connecting = 1;
            public const int Connected = 2;
        }
    }
}