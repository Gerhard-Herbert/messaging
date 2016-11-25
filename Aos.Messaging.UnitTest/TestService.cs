//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------
using System;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging.UnitTest
{
    public class TestService : IService
    {
        public Action<Message> Subscription { get; set; }

        public Message RequestMessage { get; set; }
        public Message ResponseMessage { get; set; }

        public void Request(Message message, Action<Message> onResponse, Action<Message> onError)
        {
            RequestMessage = message;
            ResponseMessage = Message.CreateResponse(message, new JValue("OK"));
            onResponse(ResponseMessage);
        }

        public void Subscribe(string messageCommand, Action<Message> onMessage)
        {
            this.Subscription = onMessage;
        }

        public void Unsubscribe(string messageCommand, Action<Message> onMessage)
        {
            if (this.Subscription != onMessage)
            {
                throw new Exception("Can't unsubscribe if no subsctiption was done.");
            }

            this.Subscription = null;
        }

        public void Notify(string notificationMessage)
        {
            var notification = Message.CreateNotification(notificationMessage);

            this.Subscription?.Invoke(notification);
        }
    }
}