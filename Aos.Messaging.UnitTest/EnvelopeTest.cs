//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class EnvelopeTest
    {
        [TestMethod]
        public void Envelope_PackUnpackTest()
        {
            Message message = Message.CreateRequest("TestRequest");
            var packed = Envelope.Pack(message, string.Empty);
            var unpacked = Envelope.Unpack(packed);
            Assert.AreEqual(message.ToString(), unpacked.Message.ToString());
        }
        [TestMethod]
        public void Envelope_UnpackNotificationTest()
        {
            Message message = Message.CreateNotification("notification");
            var packed = Envelope.Pack(message, string.Empty);
            var unpacked = Envelope.Unpack(packed);
            Assert.AreEqual(message.ToString(), unpacked.Message.ToString());
        }
    }
}
