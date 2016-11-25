//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging
{
    public class MessageSerializer
    {
        public static Message FromJObject(JObject message)
        {
            Message result = new Message();

            if (message.Value<int?>("id") != null)
            {
                result.Id = Convert.ToInt32(message["id"]);
            }
            if (message.SelectToken("method") != null)
            {
                result.Method = message["method"].ToString();
            }
            if (message.SelectToken("result") != null)
            {
                result.Result = message["result"];
                result.MessageType = MessageType.Response;
            }
            if (message.SelectToken("error") != null)
            {
                result.Error = message["error"];
                result.MessageType = MessageType.Response;
            }
            if (message.SelectToken("params") != null)
            {
                result.Params = message["params"];
                result.MessageType = result.Id == 0 ? MessageType.Notification : MessageType.Request;
            }
            return result;
        }

        public static JObject ToJObject(Message message)
        {
            JObject msgObject = new JObject();
            msgObject["jsonrpc"] = "2.0";
            switch (message.MessageType)
            {
                case MessageType.Request:
                    msgObject["method"] = message.Method;
                    msgObject["id"] = message.Id;
                    msgObject["params"] = message.Params ?? message.Params;
                    break;
                case MessageType.Response:
                    msgObject["id"] = message.Id;
                    msgObject["result"] = message.Result ?? message.Result;
                    msgObject["error"] = message.Error ?? message.Error;
                    break;
                case MessageType.Notification:
                    msgObject["method"] = message.Method;
                    msgObject["params"] = message.Params;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return msgObject;
        }

        public static string Serialize(Message message)
        {
            var msgObject = ToJObject(message);
            var result = msgObject.ToString();
            return Regex.Replace(result, @"([\r\n\t ])+", string.Empty);
        }
    }
}
