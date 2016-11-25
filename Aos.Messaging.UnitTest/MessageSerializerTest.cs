//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class MessageSerializerTest
    {
        [TestMethod]
        public void MessageSerializer_SerializeRequestTest()
        {
            Message message = Message.CreateRequest("test");
            string jsonMessage = MessageSerializer.Serialize(message);
            Assert.AreEqual($@"{{""jsonrpc"":""2.0"",""method"":""test"",""id"":{message.Id},""params"":null}}", jsonMessage);
        }

        [TestMethod]
        public void MessageSerializer_SerializeRequestWithParameterTest()
        {
            Message message = Message.CreateRequest("test");
            message.Params = new JObject { { "xx", "val" } };
            string jsonMessage = MessageSerializer.Serialize(message);
            Assert.AreEqual($@"{{""jsonrpc"":""2.0"",""method"":""test"",""id"":{message.Id},""params"":{{""xx"":""val""}}}}", jsonMessage);
        }
        [TestMethod]
        public void MessageDeserializer_JsonFromObjectWithIdNull()
        {
            var jsonObject = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["result"] = "-3",
                ["id"] = null,
                ["error"] = null
            };

            var jsonMessage = MessageSerializer.FromJObject(jsonObject);
            Assert.AreEqual(MessageType.Response, jsonMessage.MessageType);
        }
    }
}