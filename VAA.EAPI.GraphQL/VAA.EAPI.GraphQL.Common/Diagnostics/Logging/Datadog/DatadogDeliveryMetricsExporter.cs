using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog
{
    /// <summary>
    /// Exports bounded Datadog delivery diagnostics through the existing Application Insights telemetry pipeline.
    /// </summary>
    internal sealed class DatadogDeliveryMetricsExporter : IHostedService, IDisposable
    {
        private static readonly TimeSpan ExportInterval = TimeSpan.FromSeconds(30);
        private readonly TelemetryClient telemetryClient;
        private readonly DatadogLoggingOptions options;
        private readonly MeterListener listener = new();
        private readonly ConcurrentDictionary<MetricKey, long> pendingMetrics = new();
        private readonly CancellationTokenSource stoppingTokenSource = new();
        private Task exportTask;

        public DatadogDeliveryMetricsExporter(TelemetryClient telemetryClient, DatadogLoggingOptions options)
        {
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            listener.InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == DatadogDeliveryMetrics.MeterName)
                    meterListener.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>(OnMeasurementRecorded);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            listener.Start();

            if (!options.IsConfigured)
                QueueMetric("vaa.eapi.datadog.delivery.configuration_failures", 1, "invalid_configuration");

            exportTask = ExportPendingMetricsAsync(stoppingTokenSource.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            listener.Dispose();
            stoppingTokenSource.Cancel();

            if (exportTask is not null)
            {
                try
                {
                    await exportTask.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingTokenSource.IsCancellationRequested)
                {
                }
            }

            FlushPendingMetrics();
        }

        public void Dispose()
        {
            listener.Dispose();
            stoppingTokenSource.Cancel();
            stoppingTokenSource.Dispose();
        }

        private void OnMeasurementRecorded(Instrument instrument, long measurement, ReadOnlySpan<KeyValuePair<string, object>> tags, object state)
        {
            var reason = "unspecified";
            foreach (var tag in tags)
            {
                if (tag.Key == "reason" && tag.Value is string value)
                {
                    reason = value;
                    break;
                }
            }

            QueueMetric(instrument.Name, measurement, reason);
        }

        internal void FlushPendingMetrics()
        {
            foreach (var pendingMetric in pendingMetrics)
            {
                if (!pendingMetrics.TryRemove(pendingMetric.Key, out var value))
                    continue;

                TrackMetric(pendingMetric.Key.Name, value, pendingMetric.Key.Reason);
            }
        }

        private async Task ExportPendingMetricsAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(ExportInterval);

            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                    FlushPendingMetrics();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        private void QueueMetric(string metricName, long value, string reason)
        {
            var key = new MetricKey(metricName, reason);
            pendingMetrics.AddOrUpdate(key, value, (_, existing) => existing + value);
        }

        private void TrackMetric(string metricName, long value, string reason)
        {
            var metric = new MetricTelemetry(metricName, value);
            metric.Properties["reason"] = reason;
            telemetryClient.TrackMetric(metric);
        }

        private readonly record struct MetricKey(string Name, string Reason);
    }
}
