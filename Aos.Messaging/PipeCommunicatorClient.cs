//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.IO.Pipes;

namespace Aos.Messaging
{
    /// <summary>
    /// Static class for creating messengers which communicate over a pipe with a server.
    /// </summary>
    public class PipeCommunicatorClient
    {
        public static IMessenger Connect(string pipeName)
        {
            return Connect(pipeName, 500);
        }

        public static IMessenger Connect(string pipeName, int timeOut)
        {
            return new PipeMessenger(pipeName, timeOut);
        }
    }
}
