//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class ReflectionServiceTest
    {
        private RootObject rootObject;
        private ReflectionService service;
        private Message response;
        private Message notification;

        [TestInitialize]
        public void TestInitialize()
        {
            this.rootObject = new RootObject();
            this.service = new ReflectionService(this.rootObject);
            this.response = null;
        }

        [TestMethod]
        public void ReflectionService_InvalidMethodTest()
        {
            Request("invalidProperty");

            Assert.IsNotNull(this.response.Error);
            Assert.AreEqual((int)JsonRpcErrors.MethodNotFound, (int)this.response.Error["code"]);
        }

        [TestMethod]
        public void ReflectionService_VoidMethodTest()
        {
            Request("void-method");
            Assert.AreEqual("OK", this.response.Result);
        }

        [TestMethod]
        public void ReflectionService_MethodWithOneParameterTest()
        {
            Request("method-with-one-parameter", "myparameter");
            Assert.AreEqual("OK", this.response.Result);
            Assert.AreEqual("myparameter", this.rootObject.StringParameter);
        }

        [TestMethod]
        public void ReflectionService_MethodWithStringReturnTest()
        {
            string expected = "ExpectedResult";
            this.rootObject.MethodStringResult = expected;
            Request("method-with-result");
            Assert.AreEqual(expected, this.response.Result);
        }

        [TestMethod]
        public void ReflectionService_MethodWithObjectReturnTest()
        {
            DataObject expected = new DataObject { StringValue = "stringValue", IntValue = 99 };
            this.rootObject.MethodObjectResult = expected;
            Request("method-with-object-result");
            Assert.AreEqual(Message.ToJObject(expected).ToString(), this.response.Result.ToString());
        }

        [TestMethod]
        public void ReflectionService_MethodWithExceptionTest()
        {
            Request("method-with-exception");
            Assert.AreEqual((int)JsonRpcErrors.ServerException, (int)this.response.Error["code"]);
            Assert.AreEqual("testException", this.response.Error["message"]);
            Assert.IsTrue(this.response.Error["data"].ToString().Contains("testException"));
        }

        [TestMethod]
        public void ReflectionService_SubscribUnsubscribeTest()
        {
            Action<Message> handler = m => this.notification = m;
            this.service.Subscribe("something-changed", handler);
            this.rootObject.ChangeSomething();
            Assert.AreEqual("something-changed", this.notification.Method);

            this.notification = null;
            this.service.Unsubscribe("something-changed", handler);
            this.rootObject.ChangeSomething();
            Assert.IsNull(this.notification);
        }

        [TestMethod]
        public void ReflectionService_SubscriptionWithDataTest()
        {
            Action<Message> handler = m => this.notification = m;
            this.service.Subscribe("something-with-getter", handler);
            Assert.AreEqual("something-with-getter", this.notification.Method);

            this.notification = null;
            this.rootObject.GetterForSubscriptionValue = "myValue";
            this.rootObject.ChangeSomethingWithGetter();
            Assert.AreEqual("something-with-getter", this.notification.Method);
            Assert.AreEqual("myValue", this.notification.Params.ToString());
        }

        [TestMethod]
        public void ReflectionService_MultipleSubscriptionTest()
        {
            Action<Message> handler = m => this.notification = m;
            this.service.Subscribe("something-changed", handler);
            this.service.Subscribe("something-changed", handler);
            this.rootObject.ChangeSomething();
            Assert.AreEqual("something-changed", this.notification.Method);

            // First unsubscribe, notification is still sent
            this.notification = null;
            this.service.Unsubscribe("something-changed", handler);
            this.rootObject.ChangeSomething();
            Assert.AreEqual("something-changed", this.notification.Method);

            // Second unsubscribe, notification is not sent anymore
            this.notification = null;
            this.service.Unsubscribe("something-changed", handler);
            this.rootObject.ChangeSomething();
            Assert.IsNull(this.notification);
        }

        [TestMethod]
        public void ReflectionService_GetRoutesTest()
        {
            var routes = this.service.GetRoutes();
            Assert.AreEqual(10, routes.Count);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "something-changed") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "void-method") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "method-with-result") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "method-with-object-result") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "test-property-get/get") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "test-property/get") != null);
            Assert.IsTrue(routes.FirstOrDefault(r => r == "test-property/set") != null);
        }

        [TestMethod]
        public void ReflectionService_SubscriptionWithMoreChannels()
        {
            Message notification2 = null;
            Action<Message> handler1 = m => this.notification = m;
            Action<Message> handler2 = m => notification2 = m;
            this.service.Subscribe("something-changed", handler1);
            this.service.Subscribe("something-changed", handler2);
            this.rootObject.ChangeSomething();
            Assert.AreEqual("something-changed", this.notification.Method);
            Assert.AreEqual("something-changed", notification2.Method);

            // Unsubscribe first handler, notification2 is still received
            this.notification = null;
            notification2 = null;
            this.service.Unsubscribe("something-changed", handler1);
            this.rootObject.ChangeSomething();
            Assert.IsNull(this.notification);
            Assert.AreEqual("something-changed", notification2.Method);

            // Unsubscribe second handler, notification is not sent anymore
            notification2 = null;
            this.service.Unsubscribe("something-changed", handler2);
            this.rootObject.ChangeSomething();
            Assert.IsNull(notification2);
        }

        [TestMethod]
        public void ReflectionService_GetPropertyTest()
        {
            Request("test-property-get/get");
            Assert.AreEqual("testProperty", this.response.Result);
            Request("test-property/set", "myTestProperty");
            Assert.AreEqual("OK", this.response.Result);
            Request("test-property/get");
            Assert.AreEqual("myTestProperty", this.response.Result);
        }

        private void Request(string method)
        {
            this.service.Request(Message.CreateRequest(method), m => this.response = m, null);
        }

        private void Request(string method, object parameter)
        {
            var request = Message.CreateRequest(method);
            request.Params = Message.ToJObject(parameter);
            this.service.Request(request, m => this.response = m, null);
        }

        public class RootObject
        {
            [Routing("something-changed")]
            public event EventHandler<EventArgs> SomethingChanged;

            [Routing("something-with-getter", true)]
            public event EventHandler<EventArgs<string>> SomethingWithGetterChanged;

            public string StringValue { get; set; }
            public int VoidMethodCalled { get; private set; }
            public string MethodStringResult { get; set; }
            public string GetterForSubscriptionValue { get; set; }
            public DataObject MethodObjectResult { get; set; }
            public string StringParameter { get; set; }

            [Routing("test-property-get")]
            public string TestPropertyGet => "testProperty";

            [Routing("test-property")]
            public string TestProperty { get; set; }

            [Routing("void-method")]
            public void VoidMethod()
            {
                VoidMethodCalled += 1;
            }

            [Routing("method-with-one-parameter")]
            public void MethodWithOneParameter(string parameter)
            {
                StringParameter = parameter;
            }

            [Routing("method-with-exception")]
            public string MethodWithException()
            {
                throw new Exception("testException");
            }

            [Routing("method-with-result")]
            public string MethodWithStringResult()
            {
                return MethodStringResult;
            }

            [Routing("something-with-getter")]
            public string GetterForSubscription()
            {
                return MethodStringResult;
            }

            [Routing("method-with-object-result")]
            public DataObject MethodWithObjectResult()
            {
                return MethodObjectResult;
            }

            public void ChangeSomething()
            {
                SomethingChanged?.Invoke(this, new EventArgs());
            }

            public void ChangeSomethingWithGetter()
            {
                SomethingWithGetterChanged?.Invoke(this, new EventArgs<string>(GetterForSubscriptionValue));
            }
        }

        public class DataObject
        {
            [JsonProperty(PropertyName = "string-value")]
            public string StringValue { get; set; }

            [JsonProperty(PropertyName = "int-value")]
            public int IntValue { get; set; }
        }
    }
}