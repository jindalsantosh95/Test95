using System.Threading.Channels;

namespace VAA.EAPI.GraphQL.Stitch.Core.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogQueue
    {
        private readonly Channel<DatadogLogEntry> channel;

        public DatadogLogQueue(DatadogLoggingOptions options)
        {
            channel = Channel.CreateBounded<DatadogLogEntry>(new BoundedChannelOptions(options.QueueCapacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public ChannelReader<DatadogLogEntry> Reader => channel.Reader;
        public bool TryWrite(DatadogLogEntry entry) => channel.Writer.TryWrite(entry);
        public void Complete() => channel.Writer.TryComplete();
    }
}
