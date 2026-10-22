using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog
{
    public sealed class DatadogLoggingOptions
    {
        private static readonly string[] ApprovedIntakeHosts =
        [
            "http-intake.logs.datadoghq.com",
            "http-intake.logs.datadoghq.eu",
            "http-intake.logs.us3.datadoghq.com",
            "http-intake.logs.us5.datadoghq.com",
            "http-intake.logs.ap1.datadoghq.com",
            "http-intake.logs.ap2.datadoghq.com",
            "http-intake.logs.uk1.datadoghq.com",
            "http-intake.logs.ddog-gov.com",
            "http-intake.logs.us2.ddog-gov.com"
        ];

        public Uri Endpoint { get; init; }
        public string ApiKey { get; init; }
        public string Service { get; init; }
        public string Environment { get; init; }
        public string Source { get; init; } = "csharp";
        public string Version { get; init; }
        public string Tags { get; init; }
        public string[] BaggageAllowList { get; init; } = [];
        public LogLevel MinimumLevel { get; init; } = LogLevel.Information;
        public int QueueCapacity { get; init; } = 5000;
        public int BatchSize { get; init; } = 100;
        public TimeSpan FlushInterval { get; init; } = TimeSpan.FromSeconds(2);

        public bool IsConfigured =>
            Endpoint is not null &&
            Endpoint.Scheme == Uri.UriSchemeHttps &&
            Endpoint.IsDefaultPort &&
            Endpoint.AbsolutePath.Equals("/api/v2/logs", StringComparison.Ordinal) &&
            Endpoint.Query.Length == 0 &&
            Endpoint.Fragment.Length == 0 &&
            ApprovedIntakeHosts.Contains(Endpoint.Host, StringComparer.OrdinalIgnoreCase) &&
            !ApiKey.IsNullOrWhiteSpace() &&
            !Service.IsNullOrWhiteSpace();

        public static DatadogLoggingOptions FromEnvironment()
        {
            var endpointValue = ConfigReader.Try(Constants.Environment.DatadogLogsEndpoint, string.Empty);
            _ = Uri.TryCreate(endpointValue, UriKind.Absolute, out var endpoint);

            var minimumLevelValue = ConfigReader.Try(Constants.Environment.DatadogMinimumLogLevel, nameof(LogLevel.Information));
            if (!Enum.TryParse(minimumLevelValue, true, out LogLevel minimumLevel))
                minimumLevel = LogLevel.Information;

            return new DatadogLoggingOptions
            {
                Endpoint = endpoint,
                ApiKey = ConfigReader.Try(Constants.Environment.DatadogApiKey, string.Empty),
                Service = ConfigReader.Try(Constants.Environment.DatadogService, string.Empty),
                Environment = ConfigReader.Try(Constants.Environment.ReleaseEnvironment, string.Empty),
                Source = ConfigReader.Try(Constants.Environment.DatadogSource, "csharp"),
                Version = ConfigReader.Try(Constants.Environment.DatadogVersion, string.Empty),
                Tags = ConfigReader.Try(Constants.Environment.DatadogTags, string.Empty),
                BaggageAllowList = ConfigReader.Try(Constants.Environment.DatadogBaggageAllowList, string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                MinimumLevel = minimumLevel,
                QueueCapacity = Math.Clamp(ConfigReader.Try(Constants.Environment.DatadogQueueCapacity, 5000), 100, 100000),
                BatchSize = Math.Clamp(ConfigReader.Try(Constants.Environment.DatadogBatchSize, 100), 1, 1000),
                FlushInterval = TimeSpan.FromMilliseconds(Math.Clamp(ConfigReader.Try(Constants.Environment.DatadogFlushIntervalMilliseconds, 2000), 100, 60000))
            };
        }
    }
}
