//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------
using System;

namespace Aos.Messaging.UnitTest
{
    public class ChannelConnectNotifierMock : IChannelConnectNotifier
    {
        private readonly IMessenger messenger;

        public ChannelConnectNotifierMock(IMessenger messenger)
        {
            this.messenger = messenger;
            PipeDisconnected += (sender, args) => { };
        }

        public event EventHandler<PipeConnectionArgs> PipeConnected;
        public event EventHandler<PipeConnectionArgs> PipeDisconnected;

        public void ConnectPipe()
        {
            PipeConnected?.Invoke(this, new PipeConnectionArgs(this.messenger));
        }

        public void Disconnect()
        {
            PipeDisconnected?.Invoke(this, new PipeConnectionArgs(this.messenger));
        }
    }
}