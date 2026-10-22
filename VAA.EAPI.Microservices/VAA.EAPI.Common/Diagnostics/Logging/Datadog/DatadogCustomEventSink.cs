using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Diagnostics.Telemetry;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Diagnostics.Logging.Datadog
{
    /// <summary>
    /// Writes business-safe application custom events directly to the Datadog log queue.
    /// </summary>
    internal sealed class DatadogCustomEventSink : ICustomEventSink
    {
        private const string EventSource = "application.custom_event";
        private const string EventSeverityProperty = "Severity";
        private const string EventMessageProperty = "Message";
        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;

        private static readonly IReadOnlyDictionary<string, string> SafePropertyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Handler"] = "event.handler",
            ["Operation"] = "event.operation",
            [EventSeverityProperty] = "event.severity",
            ["ExceptionType"] = "event.exception_type",
            ["MemberStatus"] = "event.member_status",
            ["ChallengeType"] = "event.challenge_type"
        };

        public DatadogCustomEventSink(DatadogLogQueue queue, DatadogLoggingOptions options)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void TrackEvent(string eventName, IDictionary<string, string> eventProperties)
        {
            if (!options.IsConfigured)
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
                Message = GetMessage(eventName, eventProperties),
                Status = GetStatus(logLevel),
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

        private static string GetMessage(string eventName, IDictionary<string, string> properties) =>
            properties.TryGetValue(EventMessageProperty, out var message) && !message.IsNullOrWhiteSpace()
                ? DatadogLogEnrichment.Limit(message)
                : DatadogLogEnrichment.Limit(eventName);

        private static string GetStatus(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error or LogLevel.Critical => "error",
            _ => "info"
        };
    }
}
