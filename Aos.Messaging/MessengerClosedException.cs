//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Aos.Messaging
{
    [Serializable]
    public class MessengerClosedException : Exception
    {
        public MessengerClosedException()
        {
        }

        public MessengerClosedException(string message) : base(message)
        {
        }

        public MessengerClosedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessengerClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}