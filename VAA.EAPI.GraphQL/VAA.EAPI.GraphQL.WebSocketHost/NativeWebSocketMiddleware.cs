using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class NativeWebSocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TransportOptions options;
        private readonly ILogger<NativeWebSocketMiddleware> logger;

        public NativeWebSocketMiddleware(RequestDelegate next, TransportOptions options,
            ILogger<NativeWebSocketMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals(options.Path, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
            if (context.Request.QueryString.HasValue)
            {
                context.Response.StatusCode = 403;
                return;
            }
            if (!context.WebSockets.IsWebSocketRequest ||
                !context.WebSockets.WebSocketRequestedProtocols.Contains(TransportOptions.Protocol))
            {
                context.Response.StatusCode = 426;
                return;
            }
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            lifetime.CancelAfter(TimeSpan.FromMinutes(5));
            try
            {
                using var socket = await context.WebSockets.AcceptWebSocketAsync(TransportOptions.Protocol);
                var bridge = new BridgeConnection(socket, context, next, options);
                await bridge.RunAsync(lifetime.Token);
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            catch (Exception exception)
            {
                // Do not log frame bodies, credentials, or upstream response bodies.
                logger.LogWarning("Native WebSocket transport failed: {ExceptionType}", exception.GetType().Name);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 502;
                }
                context.Abort();
            }
        }

    }
}
