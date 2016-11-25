//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities
{
    public class WaitHelpers
    {
        private const int Timeout = 4000;
        public static void WaitUntilNotEqual<T>(T notExpected, ref T actual)
        {
            for (int tick = Environment.TickCount; Environment.TickCount - tick < Timeout; Thread.Sleep(5))
            {
                if (!notExpected.Equals(actual))
                {
                    break;
                }
            }
            Assert.AreNotEqual(notExpected, actual);
        }

        public static void WaitUntilEqual<T>(T expected, ref T actual)
        {
            for (int tick = Environment.TickCount; Environment.TickCount - tick < Timeout; Thread.Sleep(5))
            {
                if (expected.Equals(actual))
                {
                    break;
                }
            }
            Assert.AreEqual(expected, actual);
        }

        public static void WaitUntilEqual<T>(T expected, Func<T> actual)
        {
            for (int tick = Environment.TickCount; Environment.TickCount - tick < Timeout; Thread.Sleep(5))
            {
                if (expected.Equals(actual.Invoke()))
                {
                    break;
                }
            }
            Assert.AreEqual(expected, actual.Invoke());
        }

        public static void WaitUntil(Func<bool> condition, int timeout = Timeout)
        {
            for (int tick = Environment.TickCount; Environment.TickCount - tick < timeout; Thread.Sleep(5))
            {
                if (condition())
                {
                    break;
                }
            }
            Assert.IsTrue(condition(), "WaitUntil failed!");
        }

        public static void WaitUntil(Func<bool> condition, string errorMessage, int timeout = Timeout)
        {
            for (int tick = Environment.TickCount; Environment.TickCount - tick < timeout; Thread.Sleep(5))
            {
                if (condition())
                {
                    break;
                }
            }
            Assert.IsTrue(condition(), errorMessage);
        }
    }
}