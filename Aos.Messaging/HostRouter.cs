//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Aos.Messaging
{
    /// <summary>
    /// Routing of messages based on the specified host.
    /// </summary>
    public class HostRouter : IMessenger
    {
        private ConcurrentDictionary<string, HostConnection> hosts = new ConcurrentDictionary<string, HostConnection>();

        public event EventHandler<MessageReceivedEvent> MessageReceived;
        public event EventHandler ConnectionReestablished;

        /// <summary>
        /// Add connection settings for a new host.
        /// </summary>
        /// <param name="host">The host name</param>
        /// <param name="messengerFactory">Factory for creating a messenger which forwards messages to the host</param>
        public void AddConnection(string host, Func<IMessenger> messengerFactory)
        {
            var hostConnection = new HostConnection { Host = host, MessengerFactory = messengerFactory };
            this.hosts[hostConnection.Host] = hostConnection;
            if (hostConnection.Messenger != null)
            {
                hostConnection.Messenger.MessageReceived += (sender, @event) => MessageReceived?.Invoke(this, @event);
                hostConnection.Messenger.ConnectionReestablished += (sender, @event) => ConnectionReestablished?.Invoke(this, @event);
            }
        }

        public void Send(string message)
        {
            var envelope = Envelope.Unpack(message);
            IMessenger messenger;
            if (!TryResolveHost(envelope.ServiceName, out messenger))
            {
                NotifyBrokenConnection(message);
                return;
            }

            Log.Debug("ScriptiongObject::SendMessage: {0}", message);

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    messenger.Send(message);
                }
                catch (ObjectDisposedException)
                {
                    NotifyBrokenConnection(message);
                }
                catch (TimeoutException)
                {
                    NotifyBrokenConnection(message);
                }
                catch (Exception ex)
                {
                    Log.Error("SendMessage failed.", ex);
                }
            });
        }

        private bool TryResolveHost(string host, out IMessenger messenger)
        {
            HostConnection hostConnection;
            if (!this.hosts.TryGetValue(host, out hostConnection))
            {
                throw new Exception($"Could not resolve host {host}");
            }
            if (hostConnection.Messenger == null && hostConnection.MessengerFactory != null)
            {
                TryConnect(hostConnection);
            }
            messenger = hostConnection.Messenger;
            return messenger != null;
        }

        private void NotifyBrokenConnection(string message)
        {
            var request = Envelope.Unpack(message);
            var responseMessage = Message.CreateFailedResponse(request.Message, JsonRpcErrors.ConnectionBroken, "Can't connect to the service.");
            MessageReceived?.Invoke(this, new MessageReceivedEvent { Message = Envelope.Pack(responseMessage, string.Empty) });
        }

        private void TryConnect(HostConnection hostConnection)
        {
            try
            {
                hostConnection.Messenger = hostConnection.MessengerFactory();
                hostConnection.Messenger.MessageReceived += (sender, @event) => MessageReceived?.Invoke(this, @event);
                hostConnection.Messenger.ConnectionReestablished += (sender, @event) => ConnectionReestablished?.Invoke(this, @event);
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }
        }

        public void Dispose()
        {
            foreach (var hostConnection in this.hosts.Values)
            {
                if (hostConnection.Messenger != null)
                {
                    hostConnection.Messenger.Dispose();
                }
            }
        }

        private class HostConnection
        {
            public string Host { get; set; }
            public IMessenger Messenger { get; set; }
            public Func<IMessenger> MessengerFactory { get; set; }
        }
    }
}