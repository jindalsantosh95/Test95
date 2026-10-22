using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class NativeSseMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TransportOptions options;
        private readonly ILogger<NativeSseMiddleware> logger;

        public NativeSseMiddleware(RequestDelegate next, TransportOptions options,
            ILogger<NativeSseMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals(options.SsePath, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
            context.Response.Headers.CacheControl = "no-store";
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.Headers.Allow = "POST";
                context.Response.StatusCode = 405;
                return;
            }
            if (!context.Request.GetTypedHeaders().Accept?.Any(x =>
                x.MediaType.Equals("text/event-stream", StringComparison.OrdinalIgnoreCase) && x.Quality != 0) ?? true)
            {
                context.Response.StatusCode = 406;
                return;
            }
            if (context.Request.ContentType?.Split(';')[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase) != true)
            {
                context.Response.StatusCode = 415;
                return;
            }
            if (context.Request.QueryString.HasValue) { context.Response.StatusCode = 400; return; }
            if (context.Request.Headers.TryGetValue("Authorization", out var authorization) &&
                (authorization.ToString().Trim().Length == 0 || !authorization.ToString().Split(',').All(value => AuthenticationHeaderValue.TryParse(value, out _))))
            {
                context.Response.StatusCode = 401;
                return;
            }

            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            lifetime.CancelAfter(TimeSpan.FromMinutes(5));
            var bridge = new SubscriptionBridgeClient(context, next, options,
                Guid.NewGuid(), context.Request.Headers.ContainsKey("Authorization") ? context.Request.Headers.Authorization.ToString() : null, true);
            string? session = null;
            try
            {
                var bytes = new byte[65537];
                var count = await context.Request.Body.ReadAtLeastAsync(bytes, bytes.Length, false, lifetime.Token);
                if (count > 65536) { context.Response.StatusCode = 413; return; }
                using var reader = new JsonTextReader(new StringReader(Encoding.UTF8.GetString(bytes, 0, count)))
                {
                    MaxDepth = 64,
                    DateParseHandling = DateParseHandling.None
                };
                var input = JObject.Load(reader);
                if (reader.Read()) { context.Response.StatusCode = 400; return; }
                if (input["query"]?.Type != JTokenType.String ||
                    input["operationName"] is JToken operation && operation.Type is not (JTokenType.String or JTokenType.Null) ||
                    input["variables"] is JToken variables && variables.Type is not (JTokenType.Object or JTokenType.Null))
                {
                    context.Response.StatusCode = 400;
                    return;
                }
                var start = new JObject { ["command"] = "start" };
                foreach (var name in new[] { "query", "operationName", "variables", "extensions" })
                {
                    if (input.TryGetValue(name, out var value)) { start[name] = value.DeepClone(); }
                }
                await bridge.CallAsync(new JObject { ["command"] = "init" }, lifetime.Token);
                var reply = await bridge.CallAsync(start, lifetime.Token);
                session = reply.Value<string>("sessionId");
                context.Response.ContentType = "text/event-stream; charset=utf-8";
                context.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
                await context.Response.StartAsync(lifetime.Token);
                if (reply["errors"] is JArray errors)
                {
                    await WriteEventAsync(context, "next", new JObject { ["errors"] = errors }, lifetime.Token);
                    await WriteEventAsync(context, "complete", null, lifetime.Token);
                    return;
                }
                if (!Guid.TryParse(session, out _)) { throw new InvalidDataException("Invalid bridge session."); }
                await context.Response.WriteAsync(": connected\n\n", lifetime.Token);
                await context.Response.Body.FlushAsync(lifetime.Token);
                while (!lifetime.IsCancellationRequested)
                {
                    reply = await bridge.CallAsync(new JObject { ["command"] = "read", ["sessionId"] = session }, lifetime.Token);
                    if (reply["result"] is JToken result) { await WriteEventAsync(context, "next", result, lifetime.Token); }
                    if (reply.Value<bool>("complete"))
                    {
                        await WriteEventAsync(context, "complete", null, lifetime.Token);
                        return;
                    }
                    if (!reply.HasValues)
                    {
                        await context.Response.WriteAsync(": keepalive\n\n", lifetime.Token);
                        await context.Response.Body.FlushAsync(lifetime.Token);
                    }
                }
            }
            catch (JsonException) when (!context.Response.HasStarted) { context.Response.StatusCode = 400; }
            catch (HttpRequestException exception) when (!context.Response.HasStarted && exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                context.Response.StatusCode = (int)exception.StatusCode.Value;
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { context.Abort(); }
            catch (Exception exception)
            {
                logger.LogWarning("Native SSE transport failed: {ExceptionType}", exception.GetType().Name);
                if (!context.Response.HasStarted) { context.Response.StatusCode = 502; }
                else { context.Abort(); }
            }
            finally
            {
                if (session is not null)
                {
                    using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    try { await bridge.CallAsync(new JObject { ["command"] = "stop", ["sessionId"] = session }, cleanup.Token); }
                    catch (Exception) { /* Redis session expiry cleans up when the worker is unavailable. */ }
                }
            }
        }

        private static async Task WriteEventAsync(HttpContext context, string name, JToken? payload, CancellationToken token)
        {
            var data = payload?.ToString(Formatting.None) ?? "";
            if (Encoding.UTF8.GetByteCount(data) > 1024 * 1024) { throw new InvalidDataException("Result exceeds one MiB."); }
            await context.Response.WriteAsync($"event: {name}\ndata: {data}\n\n", token);
            await context.Response.Body.FlushAsync(token);
        }
    }
}
