using Microsoft.Extensions.Logging;

namespace VAA.EAPI.Web.Common.Diagnostics.Logging.Datadog
{
    [ProviderAlias("Datadog")]
    public sealed class DatadogLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;
        private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

        internal DatadogLoggerProvider(DatadogLogQueue queue, DatadogLoggingOptions options)
        {
            this.queue = queue;
            this.options = options;
        }

        public ILogger CreateLogger(string categoryName) => new DatadogLogger(categoryName, queue, options, () => scopeProvider);
        public void SetScopeProvider(IExternalScopeProvider value) => scopeProvider = value;
        public void Dispose() { }
    }
}
