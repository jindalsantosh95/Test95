using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Security.Claims;
using VAA.EAPI.GraphQL.WebSocketHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class InternalBridgeDispatchTests
    {
        [Fact]
        public async Task ConcurrentCommandsHaveIndependentScopesAndDoNotInheritPrivilegedStateAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ScopeProbe>();
            await using var app = builder.Build();
            var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
            var caller = new DefaultHttpContext { RequestServices = app.Services };
            caller.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "admin") }, "test"));
            caller.Request.Headers["x-functions-key"] = "must-not-propagate";
            caller.Request.Headers["api-correlation-id"] = "trace-test";
            caller.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "outer"));
            accessor.HttpContext = caller;
            var scopes = new ConcurrentBag<ScopeProbe>();
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var count = 0;
            RequestDelegate dispatch = async context =>
            {
                Assert.Same(context, accessor.HttpContext);
                Assert.False(context.User.Identity?.IsAuthenticated ?? false);
                Assert.Null(context.GetEndpoint());
                Assert.False(context.Request.Headers.ContainsKey("x-functions-key"));
                Assert.Equal("trace-test", context.Request.Headers["api-correlation-id"]);
                Assert.Equal("Bearer test-token", context.Request.Headers.Authorization);
                Assert.Equal("test-bridge-secret", context.Request.Headers["X-Native-WebSocket-Bridge"]);
                Assert.Equal(TransportOptions.BridgeRoute, context.Request.Path);
                Assert.Equal("POST", context.Request.Method);
                scopes.Add(context.RequestServices.GetRequiredService<ScopeProbe>());
                if (Interlocked.Increment(ref count) == 2) { ready.SetResult(); }
                await ready.Task.WaitAsync(context.RequestAborted);
                await context.Response.WriteAsync("{\"ok\":true}");
            };
            var bridge = new SubscriptionBridgeClient(caller, dispatch,
                new TransportOptions { BridgeSecret = "test-bridge-secret" }, Guid.NewGuid(), "Bearer test-token");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                var replies = await Task.WhenAll(bridge.CallAsync(new JObject(), timeout.Token), bridge.CallAsync(new JObject(), timeout.Token));
                Assert.All(replies, reply => Assert.True(reply.Value<bool>("ok")));
                Assert.Equal(2, scopes.Distinct().Count());
                Assert.All(scopes, scope => Assert.True(scope.Disposed));
                Assert.Same(caller, accessor.HttpContext);
            }
            finally { accessor.HttpContext = null; }
        }

        [Theory]
        [InlineData("cancel")]
        [InlineData("forbidden")]
        [InlineData("oversized")]
        [InlineData("invalid-json")]
        public async Task FailedCommandsReleaseRequestScopeAsync(string failure)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddScoped<ScopeProbe>();
            await using var app = builder.Build();
            var caller = new DefaultHttpContext { RequestServices = app.Services };
            ScopeProbe? scope = null;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            RequestDelegate dispatch = async context =>
            {
                scope = context.RequestServices.GetRequiredService<ScopeProbe>();
                switch (failure)
                {
                    case "cancel":
                        cancellation.Cancel();
                        await Task.Delay(Timeout.Infinite, context.RequestAborted);
                        break;
                    case "forbidden": context.Response.StatusCode = 403; break;
                    case "oversized": await context.Response.Body.WriteAsync(new byte[2 * 1024 * 1024 + 1]); break;
                    default: await context.Response.WriteAsync("invalid-json"); break;
                }
            };
            var bridge = new SubscriptionBridgeClient(caller, dispatch, new TransportOptions(), Guid.NewGuid(), null);
            var exception = await Record.ExceptionAsync(() => bridge.CallAsync(new JObject(), cancellation.Token));
            Assert.NotNull(exception);
            if (failure == "forbidden") { Assert.Equal(System.Net.HttpStatusCode.Forbidden, Assert.IsType<HttpRequestException>(exception).StatusCode); }
            if (failure == "cancel") { Assert.IsAssignableFrom<OperationCanceledException>(exception); }
            Assert.NotNull(scope);
            Assert.True(scope.Disposed);
        }

        private sealed class ScopeProbe : IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
        }
    }
}
