using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog
{
    /// <summary>
    /// Writes the fixed, business-safe GraphQL custom-event envelope to the Datadog log queue.
    /// </summary>
    internal sealed class DatadogCustomEventSink : ICustomEventSink
    {
        private const string EventSource = "application.custom_event";
        private const string EventSeverityProperty = "Severity";

        private static readonly IReadOnlyDictionary<string, string> SafePropertyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = "event.id",
            ["Source"] = "event.origin",
            ["TimestampUtc"] = "event.timestamp",
            [EventSeverityProperty] = "event.severity"
        };

        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;

        public DatadogCustomEventSink(DatadogLogQueue queue, DatadogLoggingOptions options)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void TrackEvent(string eventName, IDictionary<string, string> eventProperties)
        {
            if (!options.IsConfigured || eventName.IsNullOrWhiteSpace() || eventProperties is null)
                return;

            var logLevel = GetLogLevel(eventProperties);
            if (logLevel < options.MinimumLevel)
                return;

            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["event.name"] = DatadogLogEnrichment.Limit(eventName),
                ["event.source"] = EventSource
            };

            foreach (var property in SafePropertyNames)
            {
                if (eventProperties.TryGetValue(property.Key, out var value) && !value.IsNullOrWhiteSpace())
                    properties[property.Value] = DatadogLogEnrichment.Limit(value);
            }

            DatadogLogEnrichment.AddExecutionContext(properties, options);

            queue.TryWrite(new DatadogLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = DatadogLogEnrichment.Limit(eventName),
                Status = DatadogLogEnrichment.GetStatus(logLevel),
                Service = options.Service,
                Source = options.Source,
                Tags = options.Tags,
                Properties = properties
            });
        }

        private static LogLevel GetLogLevel(IDictionary<string, string> properties) =>
            properties.TryGetValue(EventSeverityProperty, out var severity) && Enum.TryParse(severity, true, out LogLevel logLevel)
                ? logLevel
                : LogLevel.Information;
    }
}
