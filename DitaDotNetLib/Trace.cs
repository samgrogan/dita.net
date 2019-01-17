using System;
using System.Diagnostics;
using System.IO;

namespace DitaDotNet {
    public static class Trace {

        private static TraceLevel TraceLevel { get; set; } = TraceLevel.Warning;

        // Initialize trace output for the application
        // Default output is to the console
        public static void InitializeTrace(TraceLevel traceLevel = TraceLevel.Warning, TextWriter traceTextWriter = null) {
            TraceLevel = traceLevel;
            System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(traceTextWriter ?? System.Console.Out));
            System.Diagnostics.Trace.AutoFlush = true;
        }

        // Trace methods
        public static void TraceInformation(string message) {
            switch (TraceLevel) {
                case TraceLevel.Verbose:
                case TraceLevel.Info:
                    System.Diagnostics.Trace.TraceInformation(message);
                    break;
            }
        }

        public static void TraceWarning(string message) {
            switch (TraceLevel) {
                case TraceLevel.Verbose:
                case TraceLevel.Info:
                case TraceLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(message);
                    break;
            }
        }

        public static void TraceError(string message) {
            switch (TraceLevel) {
                case TraceLevel.Verbose:
                case TraceLevel.Info:
                case TraceLevel.Warning:
                case TraceLevel.Error:
                    System.Diagnostics.Trace.TraceError(message);
                    break;
            }
        }

        public static void TraceError(Exception ex) {
            TraceError(ex.ToString());
        }
    }
}