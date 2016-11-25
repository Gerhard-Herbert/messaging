//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class PipeCommunicatorTest
    {
        private IMessenger serverMessenger;
        private IMessenger clientMessenger;
        private PipeCommunicatorServer server;
        private List<string> serverReceivedMessages = new List<string>();
        private List<string> clientReceivedMessages = new List<string>();

        [TestInitialize]
        public void TestInitialize()
        {
            string pipeName = "2EB256B1-F04F-49FB-97F6-099358AC3195";
            this.server = new PipeCommunicatorServer(pipeName);

            this.server.PipeConnected += (sender, @event) =>
            {
                this.serverMessenger = @event.Messenger;
                this.serverMessenger.MessageReceived += (o, e) => this.serverReceivedMessages.Add(e.Message);
            };

            this.server.Start();
            this.clientMessenger = PipeCommunicatorClient.Connect(pipeName);
            this.clientMessenger.MessageReceived += (sender, @event) => this.clientReceivedMessages.Add(@event.Message);
            WaitHelpers.WaitUntil(() => this.serverMessenger != null);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.server.Dispose();
            this.serverMessenger.Dispose();
            this.clientMessenger.Dispose();
            this.serverReceivedMessages.Clear();
        }

        [TestMethod]
        public void PipeCommunicator_ClientSendMessageTest()
        {
            this.clientMessenger.Send("TestMessage");

            WaitHelpers.WaitUntil(() => this.serverReceivedMessages.Count == 1);
            Assert.AreEqual("TestMessage", this.serverReceivedMessages[0]);
        }

        [TestMethod]
        public void PipeCommunicator_ServerSendMessageTest()
        {
            this.serverMessenger.Send("TestMessage");

            WaitHelpers.WaitUntil(() => this.clientReceivedMessages.Count == 1);
            Assert.AreEqual("TestMessage", this.clientReceivedMessages[0]);
        }

        [TestMethod]
        public void PipeCommunicator_ClientSendTwoMessagesTest()
        {
            this.clientMessenger.Send("TestMessage1");
            this.clientMessenger.Send("TestMessage2");

            WaitHelpers.WaitUntil(() => this.serverReceivedMessages.Count == 2);
            Assert.AreEqual("TestMessage1", this.serverReceivedMessages[0]);
            Assert.AreEqual("TestMessage2", this.serverReceivedMessages[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void PipeCommunicator_ServerNotRunningTest()
        {
            var msg = PipeCommunicatorClient.Connect("notExistingPipe", 10);
            msg.Send("TestMessage1");
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void PipeCommunicator_ServerClosedTest()
        {
            this.server.Dispose();
            this.clientMessenger.Send("TestMessage1");
        }
        
        [TestMethod]
        public void PipeCommunicator_AcceptanceTest()
        {
            this.clientMessenger.Send("ClientMessage1");
            this.serverMessenger.Send("ServerMessage1");
            this.clientMessenger.Send("ClientMessage2");
            this.clientMessenger.Send("ClientMessage3");
            this.serverMessenger.Send("ServerMessage3");
            this.clientMessenger.Send("ClientMessage4");
            this.serverMessenger.Send("ServerMessage2");
            this.serverMessenger.Send("ServerMessage4");
            this.serverMessenger.Send("ServerMessage5");
            this.clientMessenger.Send("ClientMessage5");

            WaitHelpers.WaitUntil(() => this.serverReceivedMessages.Count == 5);
            WaitHelpers.WaitUntil(() => this.clientReceivedMessages.Count == 5);

            Assert.IsTrue(this.serverReceivedMessages.Contains("ClientMessage1"));
            Assert.IsTrue(this.serverReceivedMessages.Contains("ClientMessage5"));
            Assert.IsTrue(this.clientReceivedMessages.Contains("ServerMessage1"));
            Assert.IsTrue(this.clientReceivedMessages.Contains("ServerMessage5"));
        }
    }
}
