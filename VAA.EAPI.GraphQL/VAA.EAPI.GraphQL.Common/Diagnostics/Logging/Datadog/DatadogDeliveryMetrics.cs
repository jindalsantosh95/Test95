using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog
{
    internal static class DatadogDeliveryMetrics
    {
        internal const string MeterName = "VAA.EAPI.GraphQL.Datadog";
        private static readonly Meter Meter = new(MeterName, "1.0.0");
        private static readonly Counter<long> RejectedEntries = Meter.CreateCounter<long>("vaa.eapi.datadog.delivery.rejected_entries");
        private static readonly Counter<long> DeliveryFailures = Meter.CreateCounter<long>("vaa.eapi.datadog.delivery.failures");
        private static readonly Counter<long> ConfigurationFailures = Meter.CreateCounter<long>("vaa.eapi.datadog.delivery.configuration_failures");

        internal static void RecordQueueRejected() => RejectedEntries.Add(1, new KeyValuePair<string, object>("reason", "queue_rejected"));
        internal static void RecordSinkFailure() => DeliveryFailures.Add(1, new KeyValuePair<string, object>("reason", "sink_failure"));
        internal static void RecordPermanentFailure() => DeliveryFailures.Add(1, new KeyValuePair<string, object>("reason", "permanent_response"));
        internal static void RecordRetryExhausted() => DeliveryFailures.Add(1, new KeyValuePair<string, object>("reason", "retry_exhausted"));
        internal static void RecordInvalidConfiguration() => ConfigurationFailures.Add(1, new KeyValuePair<string, object>("reason", "invalid_configuration"));
    }
}
