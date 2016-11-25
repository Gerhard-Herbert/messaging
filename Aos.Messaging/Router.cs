//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Aos.Messaging
{
    /// <summary>
    /// Dispatch messages to registered services.
    /// </summary>
    public class Router
    {
        private ConcurrentDictionary<string, IService> routes = new ConcurrentDictionary<string, IService>();
        private IMessenger messenger;
        private ConcurrentDictionary<IMessenger, Action<Message>> subscribers = new ConcurrentDictionary<IMessenger, Action<Message>>();

        public Router(IMessenger messenger)
        {
            this.messenger = messenger;
            this.messenger.MessageReceived += MessengerOnMessageReceived;
        }

        private void MessengerOnMessageReceived(object sender, MessageReceivedEvent messageReceivedEvent)
        {
            Envelope envelope = Envelope.Unpack(messageReceivedEvent.Message);
            IMessenger responseMessenger = (IMessenger)sender;
            bool handled = false;
            if (envelope.IsSubscription())
            {
                Action<Message> callback;
                if (!this.subscribers.TryGetValue(responseMessenger, out callback))
                {
                    callback = m => SendNotification(responseMessenger, m, envelope.Subscribe);
                    this.subscribers[responseMessenger] = callback;
                }

                CallService(envelope, (service) => service.Subscribe(envelope.Subscribe, callback));
                handled = true; // TODO: just ignoring if there is no handler, need to find a better fix herer
            }
            else if (envelope.IsUnsubscription())
            {
                Unsubscribe(envelope, responseMessenger);
                handled = true; // TODO: just ignoring if there is no handler, need to find a better fix herer
            }
            else if (envelope.Message != null && envelope.Message.MessageType == MessageType.Request)
            {
                handled = CallService(envelope, (service) => service.Request(envelope.Message, m => SendMessage(responseMessenger, m), null));
            }

            if (!handled)
            {
                var errorMessage = Message.CreateFailedResponse(envelope.Message, JsonRpcErrors.MethodNotFound);
                var error = Envelope.Pack(errorMessage, string.Empty);
                responseMessenger.Send(error);
            }
        }

        private void Unsubscribe(Envelope envelope, IMessenger responseMessenger)
        {
            Action<Message> callback;
            if (this.subscribers.TryGetValue(responseMessenger, out callback))
            {
                CallService(envelope, (service) => service.Unsubscribe(envelope.Unsubscribe, callback));
            }
        }

        public void SendNotification(IMessenger messenger, Message message, string service)
        {
            try
            {
                messenger.Send(Envelope.Pack(message, string.Empty));
            }
            catch (MessengerClosedException ex)
            {
                Log.Warning($"Could not send notification: {message}", ex);

                var unsubscribe = Envelope.Unpack(Envelope.CreateUnsubscribeMessage(message.Method, service));
                Unsubscribe(unsubscribe, messenger);
            }
            catch (Exception ex)
            {
                Log.Error($"Could not send notification: {message}", ex);
            }
        }

        public void SendMessage(IMessenger messenger, Message message)
        {
            try
            {
                messenger.Send(Envelope.Pack(message, string.Empty));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send message: {message}", ex);
            }
        }

        private bool CallService(Envelope envelope, Action<IService> serviceAction)
        {
            bool handled = false;
            foreach (var routeEntry in this.routes)
            {
                if (envelope.Message != null && envelope.Message.Method.StartsWith(routeEntry.Key))
                {
                    serviceAction(routeEntry.Value);
                    handled = true;
                }
                else if (envelope.IsSubscription() && envelope.Subscribe.StartsWith(routeEntry.Key))
                {
                    serviceAction(routeEntry.Value);
                    handled = true;
                }
                else if (envelope.IsUnsubscription() && envelope.Unsubscribe.StartsWith(routeEntry.Key))
                {
                    serviceAction(routeEntry.Value);
                    handled = true;
                }
            }

            return handled;
        }

        public void AddRoute(string testRoute, IService service)
        {
            this.routes[testRoute] = service;
        }

        public void AddAllRoutes(ReflectionService reflectionService)
        {
            List<string> routes = reflectionService.GetRoutes();
            foreach (var route in routes)
            {
                AddRoute(route, reflectionService);
            }
        }
    }
}
