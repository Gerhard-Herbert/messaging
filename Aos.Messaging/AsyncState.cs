//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System.IO.Pipes;

namespace Aos.Messaging
{
    /// <summary>
    /// Class used for named pipe server async connections.
    /// </summary>
    internal struct AsyncState
    {
        public PipeStream Server;
        public byte[] Len;

        public AsyncState(PipeStream server, byte[] len)
            : this()
        {
            this.Server = server;
            this.Len = len;
        }
    }
}