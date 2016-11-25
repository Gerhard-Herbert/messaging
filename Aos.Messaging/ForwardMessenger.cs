//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;

namespace Aos.Messaging
{
    /// <summary>
    /// Just connect two messengers.
    /// </summary>
    public class MessageConnector
    {
        private readonly ForwardMessenger source;
        private readonly ForwardMessenger destination;

        public MessageConnector()
        {
            this.source = new ForwardMessenger();
            this.destination = new ForwardMessenger();
            this.source.Receiver = this.destination;
            this.destination.Receiver = this.source;
        }

        public IMessenger Source => this.source;
        public IMessenger Destination => this.destination;

        protected class ForwardMessenger : IMessenger
        {
            public event EventHandler<MessageReceivedEvent> MessageReceived;
            public event EventHandler ConnectionReestablished;
            public ForwardMessenger Receiver { get; set; }

            public void InvokeMessageReceived(string message)
            {
                MessageReceived?.Invoke(this, new MessageReceivedEvent { Message = message });
            }

            public void Send(string message)
            {
                Receiver.InvokeMessageReceived(message);
            }

            protected virtual void OnConnectionReestablished()
            {
                ConnectionReestablished?.Invoke(this, EventArgs.Empty);
            }

            public void Dispose()
            {
            }
        }
    }
}
