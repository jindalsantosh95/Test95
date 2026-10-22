using System.Web;
using VAA.EAPI.Automation.Core.Azure.Functions;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class AsyncContextExtensions
    {
        private const char ChannelValueSeparator = ';';
        private const char QueryStringValueSeparator = '&';
        private const string EmptyTraceId = "00000000000000000000000000000000";
        private const string EmptySpanId = "0000000000000000";

        public static bool HasChannel(this AsyncContext source, string name)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            return GetChannel(source, name) is not null;
        }

        public static bool HasChannelKey(this AsyncContext source, string name, string key)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(key));

            string channel = GetChannel(source, name);

            if (channel is null)
                return false;

            channel = channel.Replace(ChannelValueSeparator, QueryStringValueSeparator);

            return HttpUtility.ParseQueryString(channel)
                              .AllKeys
                              .Any(x => x.Is(key));
        }

        public static string GetChannelKey(this AsyncContext source, string name, string key)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(key));

            string channel = GetChannel(source, name);

            if (channel is null)
                return null;

            channel = channel.Replace(ChannelValueSeparator, QueryStringValueSeparator);

            return HttpUtility.ParseQueryString(channel)[key];
        }

        private static string GetChannel(AsyncContext source, string name) =>
            source.Channel?.FirstOrDefault(x => IsChannel(x, name));

        private static bool IsChannel(string channel, string name) =>
            !channel.IsNullOrWhiteSpace() &&
            (channel.Is(name) || channel.Split(ChannelValueSeparator, StringSplitOptions.RemoveEmptyEntries)[0].Is(name));

        public static void MapToCloudEvent(this AsyncContext source, CloudEvent value)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (value is null)
                return;

            value.DiagnosticsEnabled = source.DiagnosticsEnabled;
            value.SessionId = source.SessionId;
            value.OperationId = source.OperationId;
            value.CorrelationId = source.CorrelationId;
            value.Channel = source.Channel;
        }

        public static void MapFromCloudEvent(this AsyncContext source, CloudEvent value)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (value is null)
                return;

            if (value.Channel is not null)
                source.SetChannel(value.Channel);

            source.SetDiagnosticsEnabled(value.DiagnosticsEnabled);
            source.SetSessionId(value.SessionId);
            source.SetOperationId(value.OperationId);
            source.SetCorrelationId(value.CorrelationId);
            source.SetTraceHeaders(preserveRemoteTraceContext: true);
        }

        public static string BuildW3cBaggage(this AsyncContext source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            string channel = Uri.EscapeDataString(string.Join(',', source.Channel ?? new string[0]));

            return $"{AsyncContext.APIDiagnosticsHeader}={source.DiagnosticsEnabled},{AsyncContext.APIChannelHeader}={channel},{AsyncContext.APISessionIdHeader}={source.SessionId:N},{AsyncContext.APIOperationIdHeader}={source.OperationId:N},{AsyncContext.APICorrelationIdHeader}={source.CorrelationId:N},{AsyncContext.OcpApimTraceHeader}={source.DiagnosticsEnabled}";
        }

        public static string BuildW3cTraceContext(this AsyncContext source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            /*
             W3C context trace spec defines the traceparent header as the primary means of correlating telemetry across components,
             and it uses a specific format that includes a version, trace-id, span-id, and trace-flags.
             - SessionId is the long lived token that represents the overall user session.
             - OperationId is used to correlate related requests across different components or services.
             - CorrelationId is the short lived token that represents the specific hop or request within that operation.
            */

            string trace = GetTraceId(source.OperationId);
            string span = GetSpanId(source.CorrelationId);

            return $"{AsyncContext.W3cTraceContextVersion}-{trace}-{span}-{AsyncContext.W3cTraceContextTraceFlags}";
        }

        private static string GetTraceId(Guid value)
        {
            string traceId = value.ToString("N");

            if (traceId == EmptyTraceId)
                traceId = Guid.NewGuid().ToString("N");

            return traceId;
        }

        private static string GetSpanId(Guid value)
        {
            string spanId = value.ToString("N")[..16];

            if (spanId == EmptySpanId)
                spanId = Guid.NewGuid().ToString("N")[..16];

            return spanId;
        }
    }
}
