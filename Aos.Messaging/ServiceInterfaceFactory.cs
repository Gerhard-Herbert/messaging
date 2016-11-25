//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

namespace Aos.Messaging
{
    /// <summary>
    /// Factory for creating service interfaces for remotely accessing services.
    /// </summary>
    public class ServiceInterfaceFactory
    {
        private IServiceLocator serviceLocator;
        private IMessengerFactory messengerFactory;

        public ServiceInterfaceFactory(IServiceLocator serviceLocator, IMessengerFactory messengFactory)
        {
            this.serviceLocator = serviceLocator;
            this.messengerFactory = messengFactory;
        }

        public IService CreateServiceInterface(string serviceName)
        {
            string uriString = this.serviceLocator.GetServiceUrl(serviceName);
            var messenger = this.messengerFactory.GetMessenger(uriString);

            return new ServiceInterface(messenger, serviceName);
        }
    }
}