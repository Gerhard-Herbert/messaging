//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class MessageConnectorTest
    {
        [TestMethod]
        public void MessageConnector_SendMessageTest()
        {
            var messageConnector = new MessageConnector();
            string receivedFromSource = string.Empty;
            string receivedFromDestination = string.Empty;
            messageConnector.Destination.MessageReceived += (sender, @event) => receivedFromSource = @event.Message;
            messageConnector.Source.MessageReceived += (sender, @event) => receivedFromDestination = @event.Message;
            messageConnector.Source.Send("testMessageFromSource");
            messageConnector.Destination.Send("testMessageFromDestination");

            Assert.AreEqual("testMessageFromSource", receivedFromSource);
            Assert.AreEqual("testMessageFromDestination", receivedFromDestination);
        }
    }
}