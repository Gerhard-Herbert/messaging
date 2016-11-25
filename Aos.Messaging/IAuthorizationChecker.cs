//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System.IO.Pipes;

namespace Aos.Messaging
{
    public interface IAuthorizationChecker
    {
        bool Check(NamedPipeServerStream srv);
    }
}