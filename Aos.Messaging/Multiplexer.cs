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
    /// Class used to forward messages from a number of messengers.
    /// </summary>
    public class Multiplexer : IMessenger
    {
        public Multiplexer(IChannelConnectNotifier channelFactory)
        {
            channelFactory.PipeConnected += (sender, args) =>
            {
                args.Messenger.MessageReceived += (o, @event) => MessageReceived?.Invoke(o, @event);
                args.Messenger.ConnectionReestablished += (o, @event) => ConnectionReestablished?.Invoke(o, @event);
            };
        }
        public event EventHandler<MessageReceivedEvent> MessageReceived;
        public event EventHandler ConnectionReestablished;

        public void Send(string message)
        {
            // Not needed, the router should know to wich messenger to return the response.
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}