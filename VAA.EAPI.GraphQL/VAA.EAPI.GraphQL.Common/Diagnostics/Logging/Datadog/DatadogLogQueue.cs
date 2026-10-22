using System.Threading.Channels;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogQueue
    {
        private readonly Channel<DatadogLogEntry> channel;

        public DatadogLogQueue(DatadogLoggingOptions options)
        {
            channel = Channel.CreateBounded<DatadogLogEntry>(new BoundedChannelOptions(options.QueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public ChannelReader<DatadogLogEntry> Reader => channel.Reader;
        public bool TryWrite(DatadogLogEntry entry)
        {
            if (channel.Writer.TryWrite(entry))
                return true;

            DatadogDeliveryMetrics.RecordQueueRejected();
            return false;
        }
        public void Complete() => channel.Writer.TryComplete();
    }
}
