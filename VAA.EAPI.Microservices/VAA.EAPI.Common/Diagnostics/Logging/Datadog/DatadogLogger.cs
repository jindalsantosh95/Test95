using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogger : ILogger
    {
        private const int MaximumPropertyCount = 128;
        private readonly string category;
        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;
        private readonly Func<IExternalScopeProvider> getScopeProvider;

        public DatadogLogger(string category, DatadogLogQueue queue, DatadogLoggingOptions options, Func<IExternalScopeProvider> getScopeProvider)
        {
            this.category = category;
            this.queue = queue;
            this.options = options;
            this.getScopeProvider = getScopeProvider;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => getScopeProvider().Push(state);

        public bool IsEnabled(LogLevel logLevel) => options.IsConfigured && logLevel >= options.MinimumLevel && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null)
                return;

            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["logger.name"] = category,
                ["event.id"] = eventId.Id
            };

            if (!eventId.Name.IsNullOrWhiteSpace())
                properties["event.name"] = eventId.Name;

            AddProperties(properties, state);
            getScopeProvider().ForEachScope((scope, target) => AddProperties(target, scope), properties);
            DatadogLogEnrichment.AddExecutionContext(properties, options);

            if (exception is not null)
            {
                properties["error.kind"] = exception.GetType().FullName;
                properties["error.message"] = DatadogLogEnrichment.Limit(exception.Message);
                properties["error.stack"] = DatadogLogEnrichment.Limit(exception.ToString());
            }

            queue.TryWrite(new DatadogLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = DatadogLogEnrichment.Limit(formatter(state, exception)),
                Status = GetStatus(logLevel),
                Service = options.Service,
                Source = options.Source,
                Tags = options.Tags,
                Properties = properties
            });
        }

        private static void AddProperties(IDictionary<string, object> properties, object value)
        {
            if (value is not IEnumerable<KeyValuePair<string, object>> structured)
                return;

            foreach (var item in structured.Where(item => !item.Key.Is("{OriginalFormat}")))
            {
                if (properties.Count >= MaximumPropertyCount)
                    break;

                if (item.Key.Is(AsyncContext.W3CBaggageHeader))
                    continue;

                properties[item.Key] = Normalize(item.Value);
            }
        }

        private static object Normalize(object value) => value switch
        {
            null => null,
            string text => DatadogLogEnrichment.Limit(text),
            bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal or Guid or DateTime or DateTimeOffset => value,
            _ => DatadogLogEnrichment.Limit(value.ToString())
        };

        private static string GetStatus(LogLevel value) => value switch
        {
            LogLevel.Trace or LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error or LogLevel.Critical => "error",
            _ => "info"
        };

    }
}
