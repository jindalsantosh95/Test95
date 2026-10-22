using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Common.Diagnostics.Telemetry
{
    public sealed class DefaultTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (AsyncContext.Current is null) return;

            if (telemetry.Context.Operation.Id.IsNullOrWhiteSpace())
                telemetry.Context.Operation.Id = AsyncContext.Current.W3CTraceId;

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

            if (!AsyncContext.Current.W3CTraceState.IsNullOrWhiteSpace())
                if (!properties.TryAdd(AsyncContext.W3cTraceStateHeader, AsyncContext.Current.W3CTraceState))
                    properties[AsyncContext.W3cTraceStateHeader] = AsyncContext.Current.W3CTraceState;

        }
    }
}
