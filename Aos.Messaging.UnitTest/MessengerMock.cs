//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Aos.Messaging.UnitTest
{
    public class MessengerMock : IMessenger
    {
        private readonly Dictionary<string, Message> responses = new Dictionary<string, Message>();

        public event EventHandler<MessageReceivedEvent> MessageReceived;
        public event EventHandler ConnectionReestablished;

        public string SendMessage { get; set; }

        public void ReestablishConnection()
        {
            ConnectionReestablished?.Invoke(this, EventArgs.Empty);
        }

        public void Send(string message)
        {
            SendMessage = message;
            var envelope = Envelope.Unpack(message);
            if (envelope.Message.Method != null && this.responses.ContainsKey(envelope.Message.Method))
            {
                MessageReceived?.Invoke(this, new MessageReceivedEvent { Message = Envelope.Pack(this.responses[envelope.Message.Method], string.Empty) });
            }
        }

        public void ClientSend(string message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEvent { Message = message });
        }

        public void SetResponse(string method, Message response)
        {
            this.responses[method] = response;
        }

        public void Dispose()
        {
        }
    }
}