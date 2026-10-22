using HotChocolate.Execution;
using HotChocolate.Stitching.Pipeline;
using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.GraphQL.Stitch.Core.Diagnostics;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;

namespace VAA.EAPI.GraphQL.Stitch.Core
{

    public sealed class HttpStitchingRequestInterceptor : IHttpStitchingRequestInterceptor
    {
        private const string AuthorizationHeader = "Authorization";
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

        public async ValueTask OnCreateRequestAsync(string targetSchema, IQueryRequest request, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
        {
            var incomingRequest = await AsyncContext.Current.Context.GetHttpRequestDataAsync();

            foreach (var header in incomingRequest.Headers.Where(x => x.Key.StartsWith("x-", StringComparison.OrdinalIgnoreCase) &&
                !x.Key.Equals("X-Native-WebSocket-Bridge", StringComparison.OrdinalIgnoreCase)))
                requestMessage.Headers.AddOrUpdate(header.Key, header.Value);

            // force the following headers to be present
            requestMessage.Headers.AddOrUpdate(AuthorizationHeader, AsyncContext.Current.Context.GetHeader(AuthorizationHeader));
            requestMessage.Headers.AddOrUpdate(OcpApimSubscriptionKeyHeader, AsyncContext.Current.Context.GetHeader(OcpApimSubscriptionKeyHeader));

            var defaultCustomProperties = Trace<HttpRequestInterceptor>.DefaultCustomProperties;

            requestMessage.Headers.AddOrUpdate(AsyncContext.APIDiagnosticsHeader, defaultCustomProperties[AsyncContext.APIDiagnosticsHeader].ToString());
            requestMessage.Headers.AddOrUpdate(AsyncContext.APIChannelHeader, defaultCustomProperties[AsyncContext.APIChannelHeader].ToString());
            requestMessage.Headers.AddOrUpdate(AsyncContext.APIOperationIdHeader, defaultCustomProperties[AsyncContext.APIOperationIdHeader].ToString());
            requestMessage.Headers.AddOrUpdate(AsyncContext.APISessionIdHeader, defaultCustomProperties[AsyncContext.APISessionIdHeader].ToString());
            requestMessage.Headers.AddOrUpdate(AsyncContext.APICorrelationIdHeader, defaultCustomProperties[AsyncContext.APICorrelationIdHeader].ToString());
            requestMessage.Headers.AddOrUpdate(AsyncContext.OcpApimTraceHeader, defaultCustomProperties[AsyncContext.OcpApimTraceHeader].ToString());
        }

        public ValueTask<IQueryResult> OnReceivedResultAsync(string targetSchema, IQueryRequest request, IQueryResult result, HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
        {
            foreach (var headerName in GetResponseHeadersToPropagate())
                CaptureResponseHeader(responseMessage, headerName);

            return new ValueTask<IQueryResult>(result);
        }

        private static readonly Lazy<string[]> ResponseHeadersToPropagate = new(() =>
            ConfigReader.Try(Constants.Environment.GraphqlResponseHeaders, string.Empty)
                       .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Where(headerName => !headerName.IsNullOrWhiteSpace())
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToArray());

        private static IEnumerable<string> GetResponseHeadersToPropagate() => ResponseHeadersToPropagate.Value;

        private static void CaptureResponseHeader(HttpResponseMessage responseMessage, string headerName)
        {
            if (responseMessage is null) return;
            if (!responseMessage.Headers.TryGetValues(headerName, out var values) &&
                !(responseMessage.Content?.Headers.TryGetValues(headerName, out values) ?? false))
                return;

            var context = AsyncContext.Current?.Context;
            if (context is null) return;

            Dictionary<string, List<string>> responseHeaders;

            lock (context.Items)
            {
                if (!context.Items.TryGetValue(Constants.Application.ResponseHeaderKey, out var item) || item is not Dictionary<string, List<string>> capturedHeaders)
                {
                    capturedHeaders = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    context.Items[Constants.Application.ResponseHeaderKey] = capturedHeaders;
                }

                responseHeaders = capturedHeaders;
            }

            lock (responseHeaders)
            {
                if (!responseHeaders.TryGetValue(headerName, out var headerValues))
                {
                    headerValues = [];
                    responseHeaders[headerName] = headerValues;
                }

                headerValues.AddRange(values.Where(value => !value.IsNullOrWhiteSpace()));
            }
        }
    }
}
