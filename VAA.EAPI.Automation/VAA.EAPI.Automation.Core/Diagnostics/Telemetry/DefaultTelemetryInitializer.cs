using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using VAA.EAPI.Automation.Core.Azure.Functions;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Diagnostics.Telemetry
{
    public sealed class DefaultTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (AsyncContext.Current is null) return;

            // W3C trace context spec defines the traceparent header as the primary means of correlating telemetry across components,
            // but Application Insights and other telemetry platforms often use the Operation.Id for correlation.
            // In EAPI, CorrelationId represents the current request, so use it for Application Insights request correlation.
            if (telemetry.Context.Operation.Id.IsNullOrWhiteSpace())
                telemetry.Context.Operation.Id = AsyncContext.Current.CorrelationId.ToString("N");

            if (telemetry is not ISupportProperties propertyTelemetry) return;

            var properties = propertyTelemetry.Properties;

            // W3C trace context spec defines the traceparent header as the primary means of correlating telemetry across components,
            // but Application Insights and other telemetry platforms often use the Session.Id for session correlation.
            // Ensure that Session.Id is set from the AsyncContext's SessionId if it's not already set,
            // to maintain session correlation in Application Insights and similar platforms.
            if (telemetry.Context.Session.Id.IsNullOrWhiteSpace())
                telemetry.Context.Session.Id = AsyncContext.Current.SessionId.ToString();


            if (!properties.TryAdd(AsyncContext.APIDiagnosticsHeader, AsyncContext.Current.DiagnosticsEnabled.ToString()))
                properties[AsyncContext.APIDiagnosticsHeader] = AsyncContext.Current.DiagnosticsEnabled.ToString();

            if (!properties.TryAdd(AsyncContext.APIChannelHeader, string.Join(',', AsyncContext.Current.Channel)))
                properties[AsyncContext.APIChannelHeader] = string.Join(',', AsyncContext.Current.Channel);

            if (!properties.TryAdd(AsyncContext.APISessionIdHeader, AsyncContext.Current.SessionId.ToString()))
                properties[AsyncContext.APISessionIdHeader] = AsyncContext.Current.SessionId.ToString();

            if (!properties.TryAdd(AsyncContext.APIOperationIdHeader, AsyncContext.Current.OperationId.ToString()))
                properties[AsyncContext.APIOperationIdHeader] = AsyncContext.Current.OperationId.ToString();

            if (!properties.TryAdd(AsyncContext.APICorrelationIdHeader, AsyncContext.Current.CorrelationId.ToString()))
                properties[AsyncContext.APICorrelationIdHeader] = AsyncContext.Current.CorrelationId.ToString();

            if (!properties.TryAdd(AsyncContext.OcpApimTraceHeader, AsyncContext.Current.DiagnosticsEnabled.ToString()))
                properties[AsyncContext.OcpApimTraceHeader] = AsyncContext.Current.DiagnosticsEnabled.ToString();

            if (!properties.TryAdd(AsyncContext.W3cTraceContextHeader, AsyncContext.Current.W3CTraceParent))
                properties[AsyncContext.W3cTraceContextHeader] = AsyncContext.Current.W3CTraceParent;

            if (!properties.TryAdd(AsyncContext.W3CBaggageHeader, AsyncContext.Current.W3CBaggage))
                properties[AsyncContext.W3CBaggageHeader] = AsyncContext.Current.W3CBaggage;
        }
    }
}

