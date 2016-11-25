//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aos.Messaging
{
    /// <summary>
    /// Implementation of the service locator interface.
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {
        private Dictionary<string, string> sercices = new Dictionary<string, string>();
        public string GetServiceUrl(string serviceName)
        {
            string url = string.Empty;
            this.sercices.TryGetValue(serviceName, out url);
            return url;
        }

        public void RegisterService(string servicename, string url)
        {
            this.sercices.Add(servicename, url);
        }
    }
}