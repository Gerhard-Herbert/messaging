//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class ServiceInterfaceFactoryTest
    {
        private MessengerMock messenger;
        private ServiceInterfaceFactory serviceFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            Mock<IServiceLocator> serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(m => m.GetServiceUrl("TestService")).Returns("pipe://testUrl/TestService");

            this.messenger = new MessengerMock();

            Mock<IMessengerFactory> messengerFactory = new Mock<IMessengerFactory>();
            messengerFactory.Setup(m => m.GetMessenger(It.IsAny<string>())).Returns(this.messenger);
            this.serviceFactory = new ServiceInterfaceFactory(serviceLocator.Object, messengerFactory.Object);
        }

        [TestMethod]
        public void ServiceInterface_RequestResponseTest()
        {
            Message request = Message.CreateRequest("TestRequest");
            Message response = Message.CreateResponse(request, new JValue("OK"));
            this.messenger.SetResponse("TestRequest", response);

            IService service = this.serviceFactory.CreateServiceInterface("TestService");
            Message receivedResponse = null;
            service.Request(request, m => receivedResponse = m, null);
            Assert.AreEqual(response.ToString(), receivedResponse.ToString());
        }
    }
}
