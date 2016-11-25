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
    /// Interface which notifies if a channel connection was established.
    /// </summary>
    public interface IChannelConnectNotifier
    {
        event EventHandler<PipeConnectionArgs> PipeConnected;
        event EventHandler<PipeConnectionArgs> PipeDisconnected;
    }
}