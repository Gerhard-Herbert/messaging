//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Aos.Messaging
{
    /// <summary>
    /// Interface for remote access to services.
    /// </summary>
    public class ServiceInterface : IService
    {
        private readonly IMessenger messenger;
        private readonly string serviceName;
        private readonly ConcurrentDictionary<int, Action<Message>> requests = new ConcurrentDictionary<int, Action<Message>>();
        private readonly ConcurrentDictionary<string, List<Action<Message>>> subscriptions = new ConcurrentDictionary<string, List<Action<Message>>>();
        public ServiceInterface(IMessenger messenger, string serviceName)
        {
            this.serviceName = serviceName;
            this.messenger = messenger;
            this.messenger.MessageReceived += Messenger_MessageReceived;
        }

        private void Messenger_MessageReceived(object sender, MessageReceivedEvent e)
        {
            Envelope envelope = Envelope.Unpack(e.Message);

            if (envelope.Message.MessageType == MessageType.Response)
            {
                Action<Message> onResponse;
                if (this.requests.TryGetValue(envelope.Message.Id, out onResponse) && onResponse != null)
                {
                    onResponse(envelope.Message);
                }
            }
            else if (envelope.Message.MessageType == MessageType.Notification)
            {
                DispatchNotification(envelope);
            }
        }

        private void DispatchNotification(Envelope envelope)
        {
            foreach (var subscription in this.subscriptions)
            {
                if (envelope.Message.Method.StartsWith(subscription.Key) && subscription.Value != null)
                {
                    CallMessageHandlers(subscription.Value, envelope.Message);
                }
            }
        }

        private void CallMessageHandlers(List<Action<Message>> messageHandlers, Message message)
        {
            foreach (var messageHandler in messageHandlers)
            {
                messageHandler(message);
            }
        }

        public void Request(Message message, Action<Message> onResponse, Action<Message> onError)
        {
            this.requests[message.Id] = onResponse;
            this.messenger.Send(Envelope.Pack(message, this.serviceName));
        }

        public void Subscribe(string messageCommand, Action<Message> onMessage)
        {
            List<Action<Message>> messageHandlers;
            if (!this.subscriptions.TryGetValue(messageCommand, out messageHandlers) || messageHandlers == null)
            {
                messageHandlers = new List<Action<Message>>();
                this.subscriptions[messageCommand] = messageHandlers;

                var subscribe = Envelope.CreateSubscribeMessage(messageCommand, this.serviceName);
                this.messenger.Send(subscribe);
            }

            messageHandlers.Add(onMessage);
        }

        public void Unsubscribe(string messageCommand, Action<Message> onMessage)
        {
            List<Action<Message>> messageHandlers;
            if (this.subscriptions.TryGetValue(messageCommand, out messageHandlers) && messageHandlers != null)
            {
                messageHandlers.Remove(onMessage);

                if (messageHandlers.Count == 0)
                {
                    var subscribe = Envelope.CreateUnsubscribeMessage(messageCommand, this.serviceName);
                    this.messenger.Send(subscribe);
                }
            }
        }
    }
}