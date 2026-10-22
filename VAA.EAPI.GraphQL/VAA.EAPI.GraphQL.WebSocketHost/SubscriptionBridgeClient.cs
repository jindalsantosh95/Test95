using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class SubscriptionBridgeClient
    {
        private readonly HttpContext caller;
        private readonly RequestDelegate dispatch;
        private readonly TransportOptions options;
        private readonly Guid connectionId;
        private readonly string? authorization;
        private readonly bool serverSentEvents;

        public SubscriptionBridgeClient(HttpContext caller, RequestDelegate dispatch, TransportOptions options,
            Guid connectionId, string? authorization, bool serverSentEvents = false)
        {
            this.caller = caller;
            this.dispatch = dispatch;
            this.options = options;
            this.connectionId = connectionId;
            this.authorization = authorization;
            this.serverSentEvents = serverSentEvents;
        }

        public Task<JObject> CallAsync(JObject body, CancellationToken cancellationToken)
        {
            var headers = new HeaderDictionary
            {
                ["X-Native-WebSocket-Bridge"] = options.BridgeSecret,
                ["Accept"] = serverSentEvents ? "text/event-stream" : "application/json"
            };
            if (authorization is not null && authorization.Length > 0) { headers["Authorization"] = authorization; }
            foreach (var name in new[] { "api-channel", "api-correlation-id", "api-session-id", "api-operation-id", "Ocp-Apim-Subscription-Key" })
            {
                if (caller.Request.Headers.TryGetValue(name, out var value)) { headers[name] = value.ToString(); }
            }
            body["connectionId"] = connectionId.ToString();
            var bytes = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));
            if (bytes.Length > 65536) { throw new InvalidDataException("Internal bridge command exceeded 64 KiB."); }
            var factory = caller.RequestServices.GetRequiredService<IHttpContextFactory>();

            // Avoid clearing the outer IHttpContextAccessor holder or inheriting request AsyncLocals.
            if (ExecutionContext.IsFlowSuppressed())
            {
                return Task.Run(() => DispatchAsync(factory, headers, bytes, cancellationToken), cancellationToken);
            }
            using (ExecutionContext.SuppressFlow())
            {
                return Task.Run(() => DispatchAsync(factory, headers, bytes, cancellationToken), cancellationToken);
            }
        }

        private async Task<JObject> DispatchAsync(IHttpContextFactory factory, IHeaderDictionary headers,
            byte[] bytes, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            using var requestBody = new MemoryStream(bytes, writable: false);
            await using var response = new BridgeResponseFeature();
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature
            {
                Method = "POST", Scheme = "http", Protocol = "HTTP/1.1",
                Path = TransportOptions.BridgeRoute, RawTarget = TransportOptions.BridgeRoute,
                PathBase = "", QueryString = "", Headers = headers, Body = requestBody
            });
            features.Set<IHttpResponseFeature>(response);
            features.Set<IHttpResponseBodyFeature>(response);
            features.Set<IHttpRequestBodyDetectionFeature>(new BridgeRequestBodyFeature());
            var context = factory.Create(features);
            try
            {
                context.Request.Host = new HostString("localhost");
                context.Request.ContentType = "application/json";
                context.Request.ContentLength = bytes.Length;
                context.RequestAborted = timeout.Token;
                context.TraceIdentifier = Guid.NewGuid().ToString("N");
                // Normal Functions-host routing, authorization and isolated-worker invocation.
                await dispatch(context);
                timeout.Token.ThrowIfCancellationRequested();
                await context.Response.CompleteAsync();
                if (context.Response.StatusCode is < 200 or >= 300)
                {
                    throw new HttpRequestException("Internal GraphQL bridge rejected the command.", null,
                        (HttpStatusCode)context.Response.StatusCode);
                }
                response.Buffer.Position = 0;
                using var reader = new JsonTextReader(new StreamReader(response.Buffer, Encoding.UTF8, leaveOpen: true))
                {
                    DateParseHandling = DateParseHandling.None, MaxDepth = 64
                };
                return JObject.Load(reader);
            }
            finally
            {
                try { await response.DisposeAsync(); }
                finally { factory.Dispose(context); }
            }
        }
    }
}
