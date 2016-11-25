//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class MessageTest
    {
        [TestMethod]
        public void Message_CreateRequestTest()
        {
            Message message = Message.CreateRequest("test");
            Assert.AreNotEqual(0, message.Id);
            Assert.AreEqual("test", message.Method);
        }

        [TestMethod]
        public void Message_CreateRequestResponseTest()
        {
            Message request = Message.CreateRequest("test");
            Message response = Message.CreateResponse(request, new JValue("OK"));
            Assert.AreEqual(request.Id, response.Id);
            Assert.AreEqual(response.Result.ToString(), "OK");
        }

        [TestMethod]
        public void ToJObjectTest()
        {
            TestObject testObject = new TestObject { TestEnum = TestObject.ETest.testEnum, TestString = "testString" };
            var jsonObject = Message.ToJObject(testObject);
            Assert.AreEqual("testString", jsonObject["TestString"]);
        }

        public class TestObject
        {
            public enum ETest
            {
                testEnum
            }

            public string TestString { get; set; }
            public ETest TestEnum { get; set; }
        }
    }
}
