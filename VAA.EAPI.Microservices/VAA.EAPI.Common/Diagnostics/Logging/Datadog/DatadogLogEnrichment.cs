using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Diagnostics.Logging.Datadog
{
    internal static class DatadogLogEnrichment
    {
        internal const int MaximumPropertyLength = 4096;

        internal static void AddExecutionContext(IDictionary<string, object> properties, DatadogLoggingOptions options)
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

        internal static string Limit(string value) => value?.Length > MaximumPropertyLength ? value[..MaximumPropertyLength] : value;

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
    }
}
