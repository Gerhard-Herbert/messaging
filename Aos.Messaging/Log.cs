//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//---------------------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace Aos.Messaging
{
    /// <summary>
    /// Wrapper class for the static Trace in order to have a more comfortabel logging.
    /// You can use trace listeners to output to different targets (nlog).
    /// NOTE: for release mode, the default listener is removed.
    /// </summary>
    public static class Log
    {
        static Log()
        {
#if !DEBUG
            Trace.Listeners.Remove("Default");
#endif
        }

        public static TraceLevel TraceLevel { get; set; }

        public static void Error(string message)
        {
            Trace.TraceError(message);
        }

        public static void Error(Exception ex)
        {
            Error("", ex);
        }

        public static void Error(string message, Exception ex)
        {
            Trace.TraceError(BuildExceptionMessage(message, ex));
        }

        public static void Error(string message, params object[] parameters)
        {
            Trace.TraceError(message, parameters);
        }

        public static void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public static void Warning(Exception ex)
        {
            Warning("", ex);
        }

        public static void Warning(string message, Exception ex)
        {
            Trace.TraceWarning(BuildExceptionMessage(message, ex));
        }

        public static void Warning(string message, params object[] parameters)
        {
            Trace.TraceWarning(message, parameters);
        }

        public static void Information(string message)
        {
            Trace.TraceInformation(message);
        }

        public static void Information(string message, params object[] parameters)
        {
            Trace.TraceInformation(message, parameters);
        }

        public static void Debug(string message)
        {
            Trace.WriteLine(message);
        }

        public static void Debug(string message, params object[] parameters)
        {
            if (TraceLevel >= TraceLevel.Verbose)
            {
                Trace.WriteLine(String.Format(message, parameters));
            }
        }

        private static string BuildExceptionMessage(string message, Exception ex)
        {
            string errorMessage = message + " " + ex.Message + " StackTrace: " + ex.StackTrace;
            if (ex.InnerException != null)
            {
                errorMessage += "InnerException: " + ex.InnerException.Message + "StackTrace: " + ex.InnerException.StackTrace;
            }
            return errorMessage;
        }
    }
}