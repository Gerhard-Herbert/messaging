//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;

namespace Aos.Messaging
{
    /// <summary>
    /// Check if the client is allowed to use the named pipe for communication.
    /// By default the client has to be signed by avira and the path of the client 
    /// needs to be the program or system folder.
    /// </summary>
    public class PipeAuthorizationChecker : IAuthorizationChecker
    {
        public PipeAuthorizationChecker()
        {
            PathList = new List<string>();
            AddDefaultPathRules();
            EnableAuthenticodeCheck = true;
        }

        public bool EnableAuthenticodeCheck { get; set; }
        internal List<string> PathList { get; }

        public bool Check(NamedPipeServerStream srv)
        {
            int clientId = GetClientProcessId(srv.SafePipeHandle.DangerousGetHandle());
            using (var clientProcess = Process.GetProcessById(clientId))
            {
                var clientPath = clientProcess.MainModule.FileName;

                return VerifyPath(clientPath);
            }
        }

        private void AddDefaultPathRules()
        {
            AddPath(Environment.GetEnvironmentVariable("ProgramFiles"));
            AddPath(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));
            AddPath(Environment.GetEnvironmentVariable("SystemRoot"));
            AddPath(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void AddPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                this.PathList.Add(path.ToLowerInvariant());
            }
        }

        private bool VerifyPath(string clientPath)
        {
            var path = clientPath.ToLowerInvariant();
            bool result = PathList.Any(pathPrefix => path.StartsWith(pathPrefix));
            if (!result)
            {
                Log.Warning("Failed to verify path for " + clientPath);
            }
            return result;
        }

        public void AllowPath(string directory)
        {
            this.PathList.Add(directory);
        }

        public int GetClientProcessId(IntPtr handle)
        {
            int id;
            if (!NativeMethods.GetNamedPipeClientProcessId(handle, out id))
            {
                throw new Win32Exception("GetNamedPipeClientProcessId failed.");
            }
            return id;
        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool GetNamedPipeClientProcessId(IntPtr handle, out int id);
        }
    }
}
