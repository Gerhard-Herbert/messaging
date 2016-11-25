//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class ServiceDispatcherTest
    {
        [TestMethod]
        public void ServiceDispatcher_RoutTest()
        {
            var messenger = new MessengerMock();
            var channelFactory = new ChannelConnectNotifierMock(messenger);
            var service = new TestService();
            var dispatcher = new Router(new Multiplexer(channelFactory));

            dispatcher.AddRoute("Test/route", service);

            channelFactory.ConnectPipe();

            var msg = Message.CreateRequest("Test/route");
            var packedMessage = Envelope.Pack(msg, "TestService");

            messenger.ClientSend(packedMessage);

            Assert.AreEqual(msg.ToString(), service.RequestMessage.ToString());
            Assert.AreEqual(Envelope.Pack(service.ResponseMessage, string.Empty), messenger.SendMessage);
        }
    }
}
