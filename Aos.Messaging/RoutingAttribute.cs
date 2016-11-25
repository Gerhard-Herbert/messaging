//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;

namespace Aos.Messaging
{
    public class RoutingAttribute : Attribute
    {
        public RoutingAttribute(string route)
        {
            Route = route;
        }
        public RoutingAttribute(string route, bool autoPublishOnSubscribe)
        {
            Route = route;
            AutoPublishOnSubscribe = autoPublishOnSubscribe;
        }
        public string Route { get; }
        public bool AutoPublishOnSubscribe { get; set; }
    }
}