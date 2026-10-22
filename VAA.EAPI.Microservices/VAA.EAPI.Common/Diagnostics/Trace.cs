using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Diagnostics
{
    public sealed class Trace<T> : IDisposable
    {
        public static Dictionary<string, object> DefaultCustomProperties => new(StringComparer.OrdinalIgnoreCase)
        {
            { AsyncContext.APIDiagnosticsHeader, AsyncContext.Current?.DiagnosticsEnabled ?? ConfigReader.Try(AsyncContext.APIDiagnosticsHeader, AsyncContext.DefaultDiagnosticsEnabled) },
            { AsyncContext.APIChannelHeader, string.Join(',', (AsyncContext.Current?.Channel ?? []).IfIsNullOrEmpty(AsyncContext.DefaultChannel)) },
            { AsyncContext.APISessionIdHeader, AsyncContext.Current?.SessionId ?? Guid.Empty },
            { AsyncContext.APIOperationIdHeader, AsyncContext.Current?.OperationId ?? Guid.Empty },
            { AsyncContext.APICorrelationIdHeader, AsyncContext.Current?.CorrelationId ?? Guid.Empty },
            { AsyncContext.OcpApimTraceHeader, AsyncContext.Current?.DiagnosticsEnabled ?? ConfigReader.Try(AsyncContext.APIDiagnosticsHeader, AsyncContext.DefaultDiagnosticsEnabled) },
            { AsyncContext.W3cTraceContextHeader, GetW3cTraceContext() },
            { AsyncContext.W3cTraceStateHeader, AsyncContext.Current?.W3CTraceState ?? string.Empty }
        };

        private static string GetW3cTraceContext()
        {
            if (AsyncContext.Current is not AsyncContext context)
                return string.Empty;

            return string.IsNullOrWhiteSpace(context.W3CTraceParent) ? context.BuildW3cTraceContext() : context.W3CTraceParent;
        }

        public static Trace<T> Create([CallerMemberName] string origin = "") => new(origin);
        public static Trace<T> Create(ILogger logger, [CallerMemberName] string origin = "") => new(logger, origin);


        private readonly Type source = typeof(T);
        private readonly Stopwatch shortScope = Stopwatch.StartNew();
        private readonly Stopwatch longScope = Stopwatch.StartNew();
        private int hashCode;

        public readonly ILogger Logger;
        public readonly string Origin;
        public readonly DebugOutput Debug;
        public readonly ConsoleOutput Console;
        public readonly LogOutput Log;
        public readonly DiagnosticsOutput Diagnostics;

        public bool DiagnosticsEnabled => (bool)DefaultCustomProperties[AsyncContext.APIDiagnosticsHeader];
        public string[] Channel => DefaultCustomProperties[AsyncContext.APIChannelHeader].ToString().Split(',');
        public Guid CorrelationId => (Guid)DefaultCustomProperties[AsyncContext.APICorrelationIdHeader];

        public string Key => $"[{hashCode}]{source.FullName}+{Origin}";
        public TimeSpan Elapsed => shortScope.Elapsed;
        public double ElapsedMilliseconds => Elapsed.TotalMilliseconds;
        public double TotalElapsedMilliseconds => longScope.ElapsedMilliseconds;

        public Trace([CallerMemberName] string origin = "") : this(null, origin) { }
        public Trace(ILogger logger, [CallerMemberName] string origin = "")
        {
            if (origin.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(origin));

            Logger = logger;
            Origin = origin;
            Debug = new(this);
            Console = new(this);
            Log = new(this);
            Diagnostics = new(this);

            this.hashCode = GetHashCode();
        }

        public void SetHashCode(int value) => this.hashCode = value;

        /// <summary>
        /// Returns custom diagnostic string with access to elapsed time span, following format indexes can be used:
        ///     {0} = Key
        ///     {1} = Elapsed time span
        ///     {2} = TotalMilliseconds
        ///     {3} = TotalSeconds
        ///     {4} = TotalMinutes
        ///     {5} = TotalHours
        ///     {6} = TotalDays
        ///     {7} = TotalElapsedMilliseconds
        /// </summary>
        /// <param name="message">custom message to be returned</param>
        /// <param name="key">Identifies stopwatch to use</param>
        /// <returns></returns>
        public string Snap(string message = "{0}> {2}ms")
        {
            string snap = string.Format(message, $"[{DateTime.UtcNow.ToString("HH:mm:ss:fff")}][{string.Join(", ", Channel)}][{CorrelationId}]{Key}", Elapsed, Elapsed.TotalMilliseconds, Elapsed.TotalSeconds, Elapsed.TotalMinutes, Elapsed.TotalHours, Elapsed.TotalDays, this.TotalElapsedMilliseconds);

            Restart();

            return snap;
        }

        public TimeSpan Stamp()
        {
            TimeSpan stamp = this.Elapsed;

            Restart();

            return stamp;
        }

        public void Restart() => shortScope.Restart();

        public void Dispose()
        {
            shortScope.Stop();
            longScope.Stop();
        }


        public sealed class DebugOutput
        {
            private readonly Trace<T> trace;

            public DebugOutput(Trace<T> trace) => this.trace = trace;

            public void WriteLine(string message = "{0}> {2}ms")
            {
                if (!Debugger.IsAttached)
                    return;

                System.Diagnostics.Debug.WriteLine(trace.Snap(message));
            }
        }

        public sealed class ConsoleOutput
        {
            private readonly Trace<T> trace;

            public ConsoleOutput(Trace<T> trace) => this.trace = trace;

            public bool IsConsole => Environment.UserInteractive && System.Console.LargestWindowWidth != 0;

            public void WriteLine(string message = "{0}> {2}ms") => System.Console.WriteLine(trace.Snap(message));
        }

        public sealed class LogOutput
        {
            private readonly Trace<T> trace;

            public LogOutput(Trace<T> trace) => this.trace = trace;

#nullable enable
            public void LogTrace(string message = "{0}> {2}ms", Dictionary<string, object>? customProperties = null)
            {
                if (trace.Logger is null) return;

                if (customProperties is not null && customProperties.Any())
                    using (trace.Logger.BeginScope(customProperties))
                        trace.Logger.LogTrace(trace.Snap(message));
                else
                    trace.Logger.LogTrace(trace.Snap(message));
            }
#nullable restore

            public void LogDebug(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                if (trace.Logger is null) return;

                if (customProperties is not null && customProperties.Any())
                    using (trace.Logger.BeginScope(customProperties))
                        trace.Logger.LogDebug(trace.Snap(message));
                else
                    trace.Logger.LogDebug(trace.Snap(message));
            }

            public void LogInformation(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                if (trace.Logger is null) return;

                if (customProperties is not null && customProperties.Any())
                    using (trace.Logger.BeginScope(customProperties))
                        trace.Logger.LogInformation(trace.Snap(message));
                else
                    trace.Logger.LogInformation(trace.Snap(message));
            }

            public void LogWarning(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                if (trace.Logger is null) return;

                if (customProperties is not null && customProperties.Any())
                    using (trace.Logger.BeginScope(customProperties))
                        trace.Logger.LogWarning(trace.Snap(message));
                else
                    trace.Logger.LogWarning(trace.Snap(message));
            }

            public void LogError(Exception exception, string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                if (trace.Logger is null) return;

                if (customProperties is not null && customProperties.Any())
                    using (trace.Logger.BeginScope(customProperties))
                        trace.Logger.LogError(exception, trace.Snap(message));
                else
                    trace.Logger.LogError(exception, trace.Snap(message));
            }
        }

        /// <summary>
        /// Diagnostics methods will execute if async context DiagnosticsEnabled is set to true.
        /// </summary>
        public sealed class DiagnosticsOutput
        {
            private readonly Trace<T> trace;

            public DiagnosticsOutput(Trace<T> trace) => this.trace = trace;

            /// <summary>
            /// Returns custom diagnostic string with access to elapsed time span, following format indexes can be used:
            ///     {0} = Key
            ///     {1} = Elapsed time span
            ///     {2} = TotalMilliseconds
            ///     {3} = TotalSeconds
            ///     {4} = TotalMinutes
            ///     {5} = TotalHours
            ///     {6} = TotalDays
            /// </summary>
            /// <param name="message">custom message to be returned</param>
            /// <param name="key">Identifies stopwatch to use</param>
            /// <returns></returns>
            public string Snap(string message = "{0}> {2}ms") => !trace.DiagnosticsEnabled ? string.Empty : trace.Snap(message);


#nullable enable
            public void LogTrace(string message = "{0}> {2}ms", Dictionary<string, object>? customProperties = null)
            {
                WriteLine(message);

                if (!trace.DiagnosticsEnabled) return;

                trace.Log.LogTrace(message, customProperties ?? DefaultCustomProperties);
            }
#nullable restore

            public void LogDebug(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                WriteLine(message);

                if (!trace.DiagnosticsEnabled) return;

                trace.Log.LogDebug(message, customProperties ?? DefaultCustomProperties);
            }

            public void LogInformation(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                WriteLine(message);

                if (!trace.DiagnosticsEnabled) return;

                trace.Log.LogInformation(message, customProperties ?? DefaultCustomProperties);
            }

            public void LogWarning(string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                WriteLine(message);

                if (!trace.DiagnosticsEnabled) return;

                trace.Log.LogWarning(message, customProperties ?? DefaultCustomProperties);
            }

            public void LogError(Exception exception, string message = "{0}> {2}ms", Dictionary<string, object> customProperties = null)
            {
                WriteLine(message);

                if (!trace.DiagnosticsEnabled) return;

                trace.Log.LogError(exception, message, customProperties ?? DefaultCustomProperties);
            }


            private void WriteLine(string message = "{0}> {2}ms")
            {
                if (trace.Console.IsConsole)
                    trace.Console.WriteLine(message);

                if (Debugger.IsAttached)
                    trace.Debug.WriteLine(message);
            }
        }
    }
}
