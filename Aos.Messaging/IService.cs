//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;

namespace Aos.Messaging
{
    public enum MessageType
    {
        Request,
        Response,
        Notification,
    }

    /// <summary>
    /// Interface for service.
    /// </summary>
    public interface IService
    {
        void Request(Message message, Action<Message> onResponse, Action<Message> onError);
        void Subscribe(string messageCommand, Action<Message> onMessage);
        void Unsubscribe(string messageCommand, Action<Message> onMessage);
    }
}
