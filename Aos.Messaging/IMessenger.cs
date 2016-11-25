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
    /// Interface used for message communication
    /// </summary>
    public interface IMessenger : IDisposable
    {
        event EventHandler<MessageReceivedEvent> MessageReceived;
        event EventHandler ConnectionReestablished;
        void Send(string message);
    }

    public class MessageReceivedEvent : EventArgs
    {
        public string Message { get; set; }
    }
}