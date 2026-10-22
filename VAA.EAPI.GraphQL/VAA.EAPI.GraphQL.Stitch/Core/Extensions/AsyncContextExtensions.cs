using System.Web;

namespace VAA.EAPI.GraphQL.Stitch.Core.Extensions
{
    public static class AsyncContextExtensions
    {
        private const char ChannelValueSeparator = ';';
        private const char QueryStringValueSeparator = '&';
        private const string EmptyTraceId = "00000000000000000000000000000000";
        private const string EmptySpanId = "0000000000000000";

        public static bool HasChannel(this AsyncContext source, string name)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            return GetChannel(source, name) is not null;
        }

        public static bool HasChannelKey(this AsyncContext source, string name, string key)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(key));

            var channel = GetChannel(source, name);

            if (channel is null) return false;

            channel = channel.Replace(ChannelValueSeparator, QueryStringValueSeparator);

            return HttpUtility.ParseQueryString(channel)
                              .AllKeys
                              .Any(x => x.Is(key));
        }

        public static string GetChannelKey(this AsyncContext source, string name, string key)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(key));

            var channel = GetChannel(source, name);

            if (channel is null) return null;

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
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (value is null) return;

            value.DiagnosticsEnabled = source.DiagnosticsEnabled;
            value.SessionId = source.SessionId;
            value.OperationId = source.OperationId;
            value.CorrelationId = source.CorrelationId;
            value.Channel = source.Channel;
        }

        public static void MapFromCloudEvent(this AsyncContext source, CloudEvent value)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (value is null) return;

            var hasContextMetadata = value.DiagnosticsEnabled ||
                                     value.Channel?.Length > 0 ||
                                     value.SessionId != Guid.Empty ||
                                     value.OperationId != Guid.Empty ||
                                     value.CorrelationId != Guid.Empty;

            if (!hasContextMetadata)
                return;

            if (value.Channel?.Length > 0)
                source.SetChannel(value.Channel);

            source.SetDiagnosticsEnabled(value.DiagnosticsEnabled);

            if (value.SessionId != Guid.Empty)
                source.SetSessionId(value.SessionId);

            if (value.OperationId != Guid.Empty)
                source.SetOperationId(value.OperationId);

            if (value.CorrelationId != Guid.Empty)
                source.SetCorrelationId(value.CorrelationId);

            source.SetTraceHeaders(preserveRemoteTraceContext: true);
        }

        public static string BuildW3cBaggage(this AsyncContext source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var channel = Uri.EscapeDataString(string.Join(',', source.Channel ?? new string[0]));

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

            var trace = GetTraceId(source.OperationId);
            var span = GetSpanId(source.CorrelationId);

            return $"{AsyncContext.W3cTraceContextVersion}-{trace}-{span}-{AsyncContext.W3cTraceContextTraceFlags}";
        }

        private static string GetTraceId(Guid value)
        {
            var traceId = value.ToString("N");

            if (traceId == EmptyTraceId)
                traceId = Guid.NewGuid().ToString("N");

            return traceId;
        }

        private static string GetSpanId(Guid value)
        {
            var spanId = value.ToString("N")[..16];

            if (spanId == EmptySpanId)
                spanId = Guid.NewGuid().ToString("N")[..16];

            return spanId;
        }
    }
}
