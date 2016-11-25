//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using Newtonsoft.Json.Linq;

namespace Aos.Messaging
{
    /// <summary>
    /// Used for packing messages and sending them over a channel.
    /// </summary>
    public class Envelope
    {
        public Message Message { get; set; }
        public string ServiceName { get; set; }
        public string Subscribe { get; set; }
        public string Unsubscribe { get; set; }

        public static string Pack(Message message, string serviceName)
        {
            JObject packed = new JObject();
            packed["message"] = MessageSerializer.ToJObject(message);
            packed["service"] = serviceName;
            return packed.ToString();
        }

        public static Envelope Unpack(string message)
        {
            JObject obj = JObject.Parse(message);
            var msg = obj["message"];
            Message result = null;
            if (msg != null)
            {
                result = MessageSerializer.FromJObject((JObject)msg);
            }
            return new Envelope
            {
                Message = result,
                ServiceName = obj["service"].ToString(),
                Subscribe = obj["subscribe"] == null ? string.Empty : obj["subscribe"].ToString(),
                Unsubscribe = obj["unsubscribe"] == null ? string.Empty : obj["unsubscribe"].ToString(),
            };
        }

        public static string CreateSubscribeMessage(string messageCommand, string serviceName)
        {
            JObject packed = new JObject();
            packed["service"] = serviceName;
            packed["subscribe"] = messageCommand;
            return packed.ToString();
        }

        public static string CreateUnsubscribeMessage(string messageCommand, string serviceName)
        {
            JObject packed = new JObject();
            packed["service"] = serviceName;
            packed["unsubscribe"] = messageCommand;
            return packed.ToString();
        }

        public bool IsSubscription()
        {
            return !string.IsNullOrEmpty(Subscribe);
        }

        public bool IsUnsubscription()
        {
            return !string.IsNullOrEmpty(Unsubscribe);
        }
    }
}