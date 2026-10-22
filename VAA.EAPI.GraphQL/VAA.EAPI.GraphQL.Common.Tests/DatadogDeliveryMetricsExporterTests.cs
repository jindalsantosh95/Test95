using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

[Collection(nameof(DatadogDeliveryMetricsCollection))]
public sealed class DatadogDeliveryMetricsExporterTests
{
    [Fact]
    public async Task FlushPendingMetricsExportsAggregatedConfigurationAndAllDeliveryFailureReasonsThroughApplicationInsights()
    {
        var channel = new CapturingTelemetryChannel();
        var options = new DatadogLoggingOptions
        {
            Endpoint = new System.Uri("https://intake.example.test/api/v2/logs"),
            ApiKey = "synthetic-key",
            Service = "graphql-test-service"
        };

        using var exporter = new DatadogDeliveryMetricsExporter(CreateTelemetryClient(channel), options);
        await exporter.StartAsync(CancellationToken.None);
        DatadogDeliveryMetrics.RecordQueueRejected();
        DatadogDeliveryMetrics.RecordQueueRejected();
        DatadogDeliveryMetrics.RecordSinkFailure();
        DatadogDeliveryMetrics.RecordPermanentFailure();
        DatadogDeliveryMetrics.RecordRetryExhausted();
        exporter.FlushPendingMetrics();

        AssertMetric(channel, "vaa.eapi.datadog.delivery.configuration_failures", "invalid_configuration", 1);
        AssertMetric(channel, "vaa.eapi.datadog.delivery.rejected_entries", "queue_rejected", 2);
        AssertMetric(channel, "vaa.eapi.datadog.delivery.failures", "sink_failure", 1);
        AssertMetric(channel, "vaa.eapi.datadog.delivery.failures", "permanent_response", 1);
        AssertMetric(channel, "vaa.eapi.datadog.delivery.failures", "retry_exhausted", 1);

        await exporter.StopAsync(CancellationToken.None);
    }

    private static void AssertMetric(CapturingTelemetryChannel channel, string name, string reason, double expectedValue)
    {
        var metric = Assert.Single(channel.Metrics, item => item.Name == name && item.Properties["reason"] == reason);
        Assert.Equal(expectedValue, metric.Sum);
    }

    private static TelemetryClient CreateTelemetryClient(CapturingTelemetryChannel telemetryChannel) => new(new TelemetryConfiguration
    {
        ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000",
        TelemetryChannel = telemetryChannel
    });

    private sealed class CapturingTelemetryChannel : ITelemetryChannel
    {
        public List<MetricTelemetry> Metrics { get; } = [];
        public bool? DeveloperMode { get; set; }
        public string EndpointAddress { get; set; } = string.Empty;

        public void Send(ITelemetry item)
        {
            if (item is MetricTelemetry metricTelemetry)
                Metrics.Add(metricTelemetry);
        }

        public void Flush() { }
        public void Dispose() { }
    }
}
