//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;

namespace Aos.Messaging
{
    /// <summary>
    /// Arguments for the pipe connection event.
    /// </summary>
    public class PipeConnectionArgs : EventArgs
    {
        public PipeConnectionArgs(IMessenger messenger)
        {
            Messenger = messenger;
        }

        public IMessenger Messenger { get; set; }
    }
}