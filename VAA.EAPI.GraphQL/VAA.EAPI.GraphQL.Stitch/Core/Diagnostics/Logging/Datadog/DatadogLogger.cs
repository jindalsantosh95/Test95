using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;

namespace VAA.EAPI.GraphQL.Stitch.Core.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogger : ILogger
    {
        private const int MaximumPropertyCount = 128;
        private const int MaximumPropertyLength = 4096;
        private readonly string category;
        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;
        private readonly Func<IExternalScopeProvider> getScopeProvider;

        public DatadogLogger(string category, DatadogLogQueue queue, DatadogLoggingOptions options, Func<IExternalScopeProvider> getScopeProvider)
        {
            this.category = category;
            this.queue = queue;
            this.options = options;
            this.getScopeProvider = getScopeProvider;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => getScopeProvider().Push(state);

        public bool IsEnabled(LogLevel logLevel) => options.IsConfigured && logLevel >= options.MinimumLevel && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null)
                return;

            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["logger.name"] = category,
                ["event.id"] = eventId.Id
            };

            if (!eventId.Name.IsNullOrWhiteSpace())
                properties["event.name"] = eventId.Name;

            AddProperties(properties, state);
            getScopeProvider().ForEachScope((scope, target) => AddProperties(target, scope), properties);
            AddExecutionContext(properties);

            if (exception is not null)
            {
                properties["error.kind"] = exception.GetType().FullName;
                properties["error.message"] = Limit(exception.Message);
                properties["error.stack"] = Limit(exception.ToString());
            }

            queue.TryWrite(new DatadogLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = Limit(formatter(state, exception)),
                Status = GetStatus(logLevel),
                Service = options.Service,
                Source = options.Source,
                Tags = options.Tags,
                Properties = properties
            });
        }

        private void AddExecutionContext(IDictionary<string, object> properties)
        {
            var context = AsyncContext.Current;
            var activity = Activity.Current;

            if (context is not null)
            {
                properties[AsyncContext.APISessionIdHeader] = context.SessionId.ToString("N");
                properties[AsyncContext.APIOperationIdHeader] = context.OperationId.ToString("N");
                properties[AsyncContext.APICorrelationIdHeader] = context.CorrelationId.ToString("N");
                properties[AsyncContext.APIChannelHeader] = string.Join(',', context.Channel ?? []);
                properties[AsyncContext.APIDiagnosticsHeader] = context.DiagnosticsEnabled;

                foreach (var item in context.GetBaggage(options.BaggageAllowList))
                    properties[$"baggage.{item.Key}"] = Limit(item.Value);
            }

            if (activity?.IdFormat == ActivityIdFormat.W3C)
            {
                var traceState = activity.TraceStateString;
                properties["w3c.trace_id"] = activity.TraceId.ToHexString();
                properties["w3c.span_id"] = activity.SpanId.ToHexString();
                AddDatadogTraceCorrelation(properties, activity.TraceId.ToHexString(), activity.SpanId.ToHexString());
                properties["w3c.trace_flags"] = activity.ActivityTraceFlags.ToString();
                properties[AsyncContext.W3cTraceContextHeader] = activity.Id;

                if (!traceState.IsNullOrWhiteSpace())
                    properties[AsyncContext.W3cTraceStateHeader] = Limit(traceState);
            }
            else if (context is not null)
            {
                properties["w3c.trace_id"] = context.W3CTraceId;
                properties["w3c.span_id"] = context.W3CSpanId;
                AddDatadogTraceCorrelation(properties, context.W3CTraceId, context.W3CSpanId);
                properties["w3c.trace_flags"] = context.W3CTraceFlags.ToString();
                properties[AsyncContext.W3cTraceContextHeader] = context.W3CTraceParent;

                if (!context.W3CTraceState.IsNullOrWhiteSpace())
                    properties[AsyncContext.W3cTraceStateHeader] = context.W3CTraceState;
            }

            if (!options.Environment.IsNullOrWhiteSpace())
                properties["dd.env"] = options.Environment;
            properties["dd.service"] = options.Service;
            if (!options.Version.IsNullOrWhiteSpace())
                properties["dd.version"] = options.Version;
        }

        private static void AddDatadogTraceCorrelation(IDictionary<string, object> properties, string traceId, string spanId)
        {
            if (TryConvertHexIdToDecimal(traceId, useLower64Bits: true, out var datadogTraceId))
                properties["dd.trace_id"] = datadogTraceId;

            if (TryConvertHexIdToDecimal(spanId, useLower64Bits: false, out var datadogSpanId))
                properties["dd.span_id"] = datadogSpanId;
        }

        private static bool TryConvertHexIdToDecimal(string value, bool useLower64Bits, out string decimalValue)
        {
            decimalValue = null;

            if (value.IsNullOrWhiteSpace())
                return false;

            var hexadecimalValue = useLower64Bits && value.Length > 16 ? value[^16..] : value;
            if (!ulong.TryParse(hexadecimalValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var parsedValue))
                return false;

            decimalValue = parsedValue.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        private static void AddProperties(IDictionary<string, object> properties, object value)
        {
            if (value is not IEnumerable<KeyValuePair<string, object>> structured)
                return;

            foreach (var item in structured.Where(item => !item.Key.Is("{OriginalFormat}")))
            {
                if (properties.Count >= MaximumPropertyCount)
                    break;

                if (item.Key.Is(AsyncContext.W3CBaggageHeader))
                    continue;

                properties[item.Key] = Normalize(item.Value);
            }
        }

        private static object Normalize(object value) => value switch
        {
            null => null,
            string text => Limit(text),
            bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal or Guid or DateTime or DateTimeOffset => value,
            _ => Limit(value.ToString())
        };

        private static string GetStatus(LogLevel value) => value switch
        {
            LogLevel.Trace or LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error or LogLevel.Critical => "error",
            _ => "info"
        };

        private static string Limit(string value) => value?.Length > MaximumPropertyLength ? value[..MaximumPropertyLength] : value;
    }
}
