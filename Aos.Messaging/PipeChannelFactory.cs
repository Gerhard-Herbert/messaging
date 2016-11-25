//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;

namespace Aos.Messaging
{
    /// <summary>
    /// Class used for creating and managing pipe connections.
    /// </summary>
    public class PipeChannelFactory : IMessengerFactory, IDisposable
    {
        private ConcurrentDictionary<string, IMessenger> messengers = new ConcurrentDictionary<string, IMessenger>();

        public IMessenger GetMessenger(string url)
        {
            Uri uri = new Uri(url);
            IMessenger result;
            if (this.messengers.TryGetValue(uri.Host, out result))
            {
                return result;
            }

            result = PipeCommunicatorClient.Connect(uri.Host);
            this.messengers[uri.Host] = result;
            return result;
        }
        public void Dispose()
        {
            foreach (var messenger in this.messengers.Values)
            {
                messenger.Dispose();
            }
        }
    }
}
