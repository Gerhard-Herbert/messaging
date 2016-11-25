//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging
{
    /// <summary>
    /// Errors as defined in the JsonRPC specification.
    /// </summary>
    public enum JsonRpcErrors
    {
        /// <summary>Invalid JSON was received by the server. An error occurred on the server while parsing the JSON text.</summary>
        ParseError = -32700,

        /// <summary>The JSON sent is not a valid Request object.</summary>
        InvalidRequest = -32600,

        /// <summary>The method does not exist / is not available..</summary>
        MethodNotFound = -32601,

        /// <summary>Invalid method parameter(s).</summary>
        InvalidParams = -32602,

        /// <summary>Internal JSON-RPC error.</summary>
        InternalError = -32603,

        /// <summary>Connection is brocken.</summary>
        ConnectionBroken = -32604,

        /// <summary>Server exception was thrown.</summary>
        ServerException = -32000
    }

    /// <summary>
    /// JsonRPC message class.
    /// </summary>
    public class Message
    {
        public string Method { get; set; }
        public int Id { get; set; }
        public JToken Params { get; set; }
        public JToken Result { get; set; }
        public JToken Error { get; set; }
        public MessageType MessageType { get; set; }

        protected static int GenerateId()
        {
            return Environment.TickCount;
        }

        public static Message CreateRequest(string method)
        {
            return new Message { Method = method, MessageType = MessageType.Request, Id = GenerateId() };
        }

        public static Message CreateResponse(Message request, JToken result)
        {
            return new Message { MessageType = MessageType.Response, Id = request.Id, Result = result };
        }

        public static Message CreateResponse(Message request, JValue result)
        {
            return new Message { MessageType = MessageType.Response, Id = request.Id, Result = result };
        }

        public static Message CreateResponse<T>(Message request, T result)
        {
            return new Message { MessageType = MessageType.Response, Id = request.Id, Result = Message.ToJObject(result) };
        }

        public static Message CreateResponse(Message request, string result)
        {
            return new Message { MessageType = MessageType.Response, Id = request.Id, Result = result };
        }

        public static Message CreateFailedResponse(Message request, JsonRpcErrors errorCode)
        {
            return CreateFailedResponse(request, errorCode, errorCode.ToString());
        }

        public static Message CreateFailedResponse(Message request, JsonRpcErrors errorCode, string errorMessage)
        {
            var error = new JObject
            {
                ["code"] = (int)errorCode,
                ["message"] = errorMessage
            };
            return new Message { Method = request?.Method ?? string.Empty, MessageType = MessageType.Response, Id = request?.Id ?? 0, Error = error };
        }

        public static Message CreateNotification(string notification)
        {
            return new Message { Method = notification, MessageType = MessageType.Notification, Id = 0 };
        }

        public override string ToString()
        {
            return MessageSerializer.Serialize(this);
        }

        /// <summary>
        /// Convert an object to a JObject.
        /// NOTE: this is pretty slow, better not use it.
        /// </summary>
        /// <typeparam name="T">The type of object to convert.</typeparam>
        /// <param name="data">The object.</param>
        /// <returns>The object converted into a JObject</returns>
        public static JToken ToJObject<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data);
            if (data is string)
            {
                return data.ToString();
            }
            return JsonConvert.DeserializeObject<JObject>(json);
        }

        public static object ToType(Type t, JToken data)
        {
            if (t == typeof(string))
            {
                return data.ToString();
            }

            string json = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject(json, t);
        }
    }
}