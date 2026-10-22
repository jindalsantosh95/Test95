using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry
{
    public class TelemetryService : ITelemetryService
    {
        private static readonly StringComparer KeyComparer = StringComparer.OrdinalIgnoreCase;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<TelemetryService> _logger;
        private readonly ICustomEventTracker _customEventTracker;

        public TelemetryService(TelemetryClient telemetryClient, ILogger<TelemetryService> logger)
            : this(telemetryClient, logger, null)
        {
        }

        public TelemetryService(TelemetryClient telemetryClient, ILogger<TelemetryService> logger, ICustomEventTracker customEventTracker)
        {
            _telemetry = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customEventTracker = customEventTracker;
        }

        public Task TrackTelemetry(TelemetryEvent telemetryEvent)
        {
            if (telemetryEvent is null)
            {
                _logger.LogWarning("TrackTelemetry called with null telemetryEvent");
                return Task.CompletedTask;
            }

            if (telemetryEvent.Type is null)
            {
                _logger.LogWarning("TrackTelemetry called with a telemetryEvent that has no type");
                return Task.CompletedTask;
            }

            var eventName = telemetryEvent.Type.ToString();
            var destination = LoggingProviderResolver.FromEnvironment();

            if (destination is LoggingProvider.AppInsights or LoggingProvider.Both)
                TrackApplicationInsights(eventName, telemetryEvent);

            if (destination is LoggingProvider.Datadog or LoggingProvider.Both)
                TrackDatadog(eventName, telemetryEvent);

            return Task.CompletedTask;
        }

        private void TrackApplicationInsights(string eventName, TelemetryEvent telemetryEvent)
        {
            try
            {
                var properties = telemetryEvent.Data?.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>(KeyComparer);
                if (properties.Comparer != KeyComparer)
                    properties = new Dictionary<string, string>(properties, KeyComparer);

                var eventTelemetry = new EventTelemetry(eventName);
                foreach (var property in properties)
                    eventTelemetry.Properties[property.Key] = property.Value;

                _telemetry.TrackEvent(eventTelemetry);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Application Insights custom-event tracking failed");
            }
        }

        private void TrackDatadog(string eventName, TelemetryEvent telemetryEvent)
        {
            if (_customEventTracker is null)
                return;

            try
            {
                _customEventTracker.TrackEvent(eventName, new Dictionary<string, string>(KeyComparer)
                {
                    ["Id"] = telemetryEvent.Id,
                    ["Source"] = telemetryEvent.Source,
                    ["TimestampUtc"] = telemetryEvent.DateTimeUtc,
                    ["Severity"] = LogLevel.Information.ToString()
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Datadog custom-event tracking failed");
            }
        }
    }
}
