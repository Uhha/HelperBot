using System;
using System.Diagnostics;

namespace Tracer
{
    public static class TraceError
    {
        public static void Info(string message)
        {
            Trace.TraceError(message);
            //var tracesource = new TraceSource("Appharbortracesource", defaultLevel: SourceLevels.All);
            //tracesource.TraceEvent(TraceEventType.Critical, 0, "Foo");
        }

        public static void Error(string message)
        {
            Trace.TraceError(message);
        }

        public static void Error(string message, string exceotion)
        {
            Trace.TraceError(message + " " + exceotion);
        }

        public static void Error(Exception e)
        {
            Trace.TraceError("ERROR: " + e.Message + " " + e.InnerException?.Message);
        }
    }
}
