using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tracer
{
    public static class TraceError
    {
        private static StringBuilder _sb = new StringBuilder();
        
        public static void Info(string message)
        {
            Trace.TraceError(message);
            var tracesource = new TraceSource("Appharbortracesource", defaultLevel: SourceLevels.All);
            tracesource.TraceEvent(TraceEventType.Critical, 0, message);
            _sb.Append($"{DateTime.Now.ToString()} : {message}<br>");
        }

        public static void Error(string message)
        {
            Trace.TraceError(message);
            _sb.Append($"{DateTime.Now.ToString()} : {message}<br>");
        }

        public static void Error(string message, string exception)
        {
            Trace.TraceError(message + " " + exception);
            _sb.Append($"{DateTime.Now.ToString()} : {message}<br>");
        }

        public static void Error(Exception e, string message = "")
        {
            //Trace.TraceError("ERROR: " + e.Message + " " + e.InnerException?.Message);
            Trace.TraceError("ERROR: " + "Custom message - " + message + Environment.NewLine + e.ToString());
            _sb.Append($"{DateTime.Now.ToString()} : {message}<br>");
        }

        public static string[] GetLog()
        {
            return (_sb != null) ? _sb.ToString().Split("<br>") : new string[0];
        }
    }
}
