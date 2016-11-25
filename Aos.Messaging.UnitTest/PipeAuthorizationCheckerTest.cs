//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace Aos.Messaging.UnitTest
{
    [TestClass]
    public class PipeAuthorizationCheckerTest
    {
        private PipeCommunicatorServer communicatorServer;
        private PipeChannelFactory channelFactory;

        [TestCleanup]
        public void TestCleanup()
        {
            this.communicatorServer?.Dispose();
            this.channelFactory?.Dispose();
        }

        [TestMethod]
        public void PipeAuthorization_CheckPathTest()
        {
            this.communicatorServer = new PipeCommunicatorServer("CA25C0E2-72DE-46DC-845B-01A92E633D2B");
            this.communicatorServer.Start();
            PipeAuthorizationChecker checker = new PipeAuthorizationChecker { EnableAuthenticodeCheck = false };
            this.communicatorServer.AuthorizationChecker = checker;

            this.channelFactory = new PipeChannelFactory();
            IMessenger messenger = this.channelFactory.GetMessenger("pipe://CA25C0E2-72DE-46DC-845B-01A92E633D2B");
            messenger.Send("test");
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void PipeAuthorization_CheckPath_InvalidPathTest()
        {
            this.communicatorServer = new PipeCommunicatorServer("CA25C0E2-72DE-46DC-845B-01A92E633D2B");
            this.communicatorServer.Start();
            PipeAuthorizationChecker checker = new PipeAuthorizationChecker();
            this.communicatorServer.AuthorizationChecker = checker;
            checker.PathList.Clear();

            this.channelFactory = new PipeChannelFactory();
            IMessenger messenger = this.channelFactory.GetMessenger("pipe://CA25C0E2-72DE-46DC-845B-01A92E633D2B");
            messenger.Send("test");
        }

        [TestMethod]
        public void PipeAuthorization_CheckSignatureTest()
        {
            this.communicatorServer = new PipeCommunicatorServer("CA25C0E2-72DE-46DC-845B-01A92E633D2B");
            this.communicatorServer.Start();
            PipeAuthorizationChecker checker = new PipeAuthorizationChecker();
            this.communicatorServer.AuthorizationChecker = checker;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PipeConnectorTest.exe");
            
            var statrupInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "CA25C0E2-72DE-46DC-845B-01A92E633D2B",
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            using (var process = Process.Start(statrupInfo))
            {
                WaitHelpers.WaitUntil(() => process.HasExited);
                Assert.AreEqual(0, process.ExitCode);
            }
        }
    }
}
