//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System.IO.Pipes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestUtilities;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class MessagingTest
    {
        private const string PipeName = "F77DE1FB-F58D-433A-B626-889FC77D096B";
        private Router router;
        private PipeCommunicatorServer communicatorServer;
        private PipeChannelFactory pipeChannelFactory;
        private TestService testService;
        private IService remoteService;

        [TestInitialize]
        public void TestInitialize()
        {
            CreateServiceDispatcherHost();
            CreateRemoteService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.testService.Subscription = null;

            this.communicatorServer.Dispose();
            this.pipeChannelFactory.Dispose();
        }

        [TestMethod]
        public void Messaging_RequestResponseTest()
        {
            this.router.AddRoute("TestRequest", this.testService);
            var request = Message.CreateRequest("TestRequest");
            var response = ExecuteRequest(request);

            Assert.AreEqual(this.testService.RequestMessage.ToString(), request.ToString());
            Assert.AreEqual(this.testService.ResponseMessage.ToString(), response.ToString());
        }

        [TestMethod]
        public void Messaging_RequestResponseUsingRouteTest()
        {
            this.router.AddRoute("Test/data", this.testService);

            var request = Message.CreateRequest("Test/data/get");
            Message response = ExecuteRequest(request);

            Assert.AreEqual(this.testService.RequestMessage.ToString(), request.ToString());
            Assert.AreEqual(this.testService.ResponseMessage.ToString(), response.ToString());
        }

        [TestMethod]
        public void Messaging_NotificationTest()
        {
            this.router.AddRoute("notification", this.testService);
            Message notification = null;

            this.remoteService.Subscribe("notification", n => notification = n);
            WaitHelpers.WaitUntil(() => this.testService.Subscription != null);

            this.testService.Notify("notification");
            WaitHelpers.WaitUntil(() => notification != null);
        }

        [TestMethod]
        public void Messaging_NotificationAfterMessengerClosedTest()
        {
            bool disconnected = false;
            this.communicatorServer.PipeDisconnected += (s, e) => disconnected = true;

            this.router.AddRoute("notification", this.testService);
            Message notification = null;

            this.remoteService.Subscribe("notification", n => notification = n);
            WaitHelpers.WaitUntil(() => this.testService.Subscription != null);

            // close the messenger
            this.pipeChannelFactory.Dispose();
            WaitHelpers.WaitUntil(() => disconnected);

            this.testService.Notify("notification");

            // notification could not be send, an automatic unsubscription was performed
            WaitHelpers.WaitUntil(() => this.testService.Subscription == null);
        }

        [TestMethod]
        public void Messaging_NotificationUsingRouteTest()
        {
            this.router.AddRoute("Test/data", this.testService);
            Message notification = null;

            this.remoteService.Subscribe("Test/data", n => notification = n);
            WaitHelpers.WaitUntil(() => this.testService.Subscription != null);

            this.testService.Notify("Test/data/hasChanged");
            WaitHelpers.WaitUntil(() => notification != null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void Messaging_AuthorizationFailedTest()
        {
            Mock<IAuthorizationChecker> authorizationChecker = new Mock<IAuthorizationChecker>();
            this.remoteService = null;

            try
            {
                authorizationChecker.Setup(m => m.Check(It.IsAny<NamedPipeServerStream>())).Returns(false);
                CreateServiceDispatcherHost();
                this.communicatorServer.AuthorizationChecker = authorizationChecker.Object;
                CreateRemoteService();
                Assert.Fail("IOException not thrown,");
            }
            catch (System.IO.IOException)
            {
            }

            // Check that the server still allows connections.
            authorizationChecker.Setup(m => m.Check(It.IsAny<NamedPipeServerStream>())).Returns(false);
            CreateRemoteService();
            Assert.IsNotNull(this.remoteService);
        }

        private Message ExecuteRequest(Message request)
        {
            Message response = null;
            this.remoteService.Request(request, r => response = r, null);

            WaitHelpers.WaitUntil(() => response != null);
            return response;
        }

        private void CreateRemoteService()
        {
            this.pipeChannelFactory?.Dispose();
            this.pipeChannelFactory = new PipeChannelFactory();
            Mock<IServiceLocator> serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(m => m.GetServiceUrl("TestService")).Returns("pipe://F77DE1FB-F58D-433A-B626-889FC77D096B/TestService");

            ServiceInterfaceFactory factory = new ServiceInterfaceFactory(serviceLocator.Object, this.pipeChannelFactory);
            this.remoteService = factory.CreateServiceInterface("TestService");
        }

        private void CreateServiceDispatcherHost()
        {
            this.communicatorServer?.Dispose();
            this.communicatorServer = new PipeCommunicatorServer(PipeName);
            this.communicatorServer.Start();

            this.testService = new TestService();
            this.router = new Router(new Multiplexer(this.communicatorServer));
        }
    }
}
