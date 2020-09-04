using System;
using System.Diagnostics;
using System.IO;

namespace Tracer
{
    public static class TraceError
    {
        public static void Info(string message)
        {
            Trace.TraceError(message);
            var tracesource = new TraceSource("Appharbortracesource", defaultLevel: SourceLevels.All);
            tracesource.TraceEvent(TraceEventType.Critical, 0, message);
            File.AppendAllText("log.txt", message);
        }

        public static void Error(string message)
        {
            Trace.TraceError(message);
        }

        public static void Error(string message, string exception)
        {
            Trace.TraceError(message + " " + exception);
        }

        public static void Error(Exception e, string message = "")
        {
            //Trace.TraceError("ERROR: " + e.Message + " " + e.InnerException?.Message);
            Trace.TraceError("ERROR: " + "Custom message - " + message + Environment.NewLine + e.ToString());
        }
    }
}
