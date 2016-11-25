//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

namespace Aos.Messaging
{
    /// <summary>
    /// Factory interface for creating messengers.
    /// </summary>
    public interface IMessengerFactory    
    {
        IMessenger GetMessenger(string url);
    }
}