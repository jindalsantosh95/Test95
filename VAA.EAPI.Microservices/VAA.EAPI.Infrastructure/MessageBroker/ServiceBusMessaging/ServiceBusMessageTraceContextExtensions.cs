using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging
{
    /// <summary>
    /// Carries the safe W3C context in Service Bus application properties.
    /// CloudEvent bodies remain unchanged, so this also works for non-CloudEvent messages.
    /// </summary>
    public static class ServiceBusMessageTraceContextExtensions
    {
        public const string DiagnosticIdApplicationProperty = "Diagnostic-Id";

        public static ServiceBusMessage AddEapiTraceContext(this ServiceBusMessage message, AsyncContext context = null)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            context ??= AsyncContext.Current;
            if (context is null)
                return message;

            SetApplicationProperty(message.ApplicationProperties, AsyncContext.W3cTraceContextHeader, context.W3CTraceParent);
            SetApplicationProperty(message.ApplicationProperties, DiagnosticIdApplicationProperty, context.W3CTraceParent);
            SetApplicationProperty(message.ApplicationProperties, AsyncContext.W3cTraceStateHeader, context.W3CTraceState);
            SetApplicationProperty(message.ApplicationProperties, AsyncContext.W3CBaggageHeader, context.GetBaggageForPropagation());

            return message;
        }

        public static void RestoreEapiTraceContext(this ServiceBusReceivedMessage message, AsyncContext context = null)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            RestoreEapiTraceContext(message.ApplicationProperties, context);
        }

        public static void RestoreEapiTraceContext(this IReadOnlyDictionary<string, object> applicationProperties, AsyncContext context = null)
        {
            if (applicationProperties is null)
                return;

            context ??= AsyncContext.Current;
            context?.RestoreTraceContext(
                GetApplicationProperty(applicationProperties, AsyncContext.W3cTraceContextHeader) ??
                    GetApplicationProperty(applicationProperties, DiagnosticIdApplicationProperty),
                GetApplicationProperty(applicationProperties, AsyncContext.W3cTraceStateHeader),
                GetApplicationProperty(applicationProperties, AsyncContext.W3CBaggageHeader));
        }

        public static void RestoreEapiTraceContext(this string serializedApplicationProperties, AsyncContext context = null)
        {
            if (serializedApplicationProperties.IsNullOrWhiteSpace())
                return;

            try
            {
                JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedApplicationProperties)?.RestoreEapiTraceContext(context);
            }
            catch (JsonException)
            {
                // Trace context must never prevent a Service Bus message from being processed.
            }
        }

        private static void SetApplicationProperty(IDictionary<string, object> applicationProperties, string name, string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                applicationProperties.Remove(name);
                return;
            }

            applicationProperties[name] = value;
        }

        private static string GetApplicationProperty(IReadOnlyDictionary<string, object> applicationProperties, string name) =>
            applicationProperties.TryGetValue(name, out var value) ? value as string : null;
    }
}
