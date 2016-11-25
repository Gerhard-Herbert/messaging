//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestUtilities;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class HostRouterTest
    {
        [TestMethod]
        public void HostRouter_DispatchMessageTest()
        {
            HostRouter hostRouter = new HostRouter();
            Mock<IMessenger> messenger1 = new Mock<IMessenger>();
            Mock<IMessenger> messenger2 = new Mock<IMessenger>();
            hostRouter.AddConnection("host1", () => messenger1.Object);
            hostRouter.AddConnection("host2", () => messenger2.Object);

            var message = Message.CreateRequest("someRequest");
            string jsonMessage = Envelope.Pack(message, "host1");
            hostRouter.Send(jsonMessage);
            messenger1.Verify(m => m.Send(jsonMessage), Times.Exactly(1));
            messenger2.Verify(m => m.Send(jsonMessage), Times.Exactly(0));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void HostRouter_HostNotAvailable()
        {
            HostRouter hostRouter = new HostRouter();

            var message = Message.CreateRequest("someRequest");
            string jsonMessage = Envelope.Pack(message, "host1");
            hostRouter.Send(jsonMessage);
        }

        [TestMethod]
        public void HostRouter_MessengerDisposedTest()
        {
            HostRouter hostRouter = new HostRouter();
            Mock<IMessenger> messenger = new Mock<IMessenger>();
            string responseMessage = null;
            hostRouter.MessageReceived += (sender, @event) => responseMessage = @event.Message;
            messenger.Setup(m => m.Send(It.IsAny<string>())).Callback(() => { throw new ObjectDisposedException(string.Empty); });
            hostRouter.AddConnection("host1", () => messenger.Object);

            var message = Message.CreateRequest("someRequest");
            string jsonMessage = Envelope.Pack(message, "host1");
            hostRouter.Send(jsonMessage);

            WaitHelpers.WaitUntil(() => responseMessage != null);
        }

        private void M_MessageReceived(object sender, MessageReceivedEvent e)
        {
            throw new NotImplementedException();
        }
    }
}