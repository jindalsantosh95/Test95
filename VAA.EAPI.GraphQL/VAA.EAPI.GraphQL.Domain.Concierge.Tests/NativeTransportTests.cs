using HotChocolate.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using VAA.EAPI.GraphQL.WebSocketHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class NativeTransportTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("Bearer test-token")]
        public async Task SsePublishesEventsBeforeCompletionAndStopsOnDisconnectAsync(string? authorization)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var client = new HttpClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using var request = new HttpRequestMessage(HttpMethod.Post, fixture.App.Urls.Single() + "/sse");
            request.Headers.Accept.ParseAdd("text/event-stream");
            if (authorization is not null) { request.Headers.TryAddWithoutValidation("Authorization", authorization); }
            request.Content = new StringContent(new JObject
            {
                ["query"] = "subscription { conciergeMessageReceived(conversationId: \"12345678-1234-1234-1234-123456789abc\") { message response } }"
            }.ToString(), Encoding.UTF8, "application/json");
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token))
            {
                response.EnsureSuccessStatusCode();
                Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(timeout.Token));
                await fixture.SubscriptionCreated.Task.WaitAsync(timeout.Token);
                foreach (var message in new[] { "ping", "pong" })
                {
                    using var mutation = await fixture.Executor.ExecuteAsync(
                        "mutation { postConciergeMessage(conversationId: \"12345678-1234-1234-1234-123456789abc\", message: \"" + message + "\") { response } }", timeout.Token);
                    string? line;
                    do { line = await reader.ReadLineAsync(timeout.Token); Assert.NotNull(line); } while (!line.StartsWith("data: "));
                    var payload = JObject.Parse(line[6..]);
                    Assert.Equal(message == "ping" ? "pong" : "ping", payload["data"]?["conciergeMessageReceived"]?["response"]?.ToString());
                }
            }
            await fixture.SubscriptionStopped.Task.WaitAsync(timeout.Token);
        }

        [Theory]
        [InlineData("GET", "application/json", "text/event-stream", "{}", 405)]
        [InlineData("POST", "text/plain", "text/event-stream", "{}", 415)]
        [InlineData("POST", "application/json", "application/json", "{}", 406)]
        [InlineData("POST", "application/json", "text/event-stream;q=0", "{}", 406)]
        [InlineData("POST", "application/json", "text/event-stream", "{broken", 400)]
        public async Task SseRejectsInvalidRequestsBeforeStartingSubscriptionAsync(string method, string contentType, string accept, string body, int status)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod(method), fixture.App.Urls.Single() + "/sse");
            request.Headers.Accept.ParseAdd(accept);
            request.Content = new StringContent(body, Encoding.UTF8, contentType);
            using var response = await client.SendAsync(request);
            Assert.Equal(status, (int)response.StatusCode);
            Assert.False(fixture.SubscriptionCreated.Task.IsCompleted);
        }

        [Fact]
        public async Task SseValidationErrorsAreNextThenCompleteAsync()
        {
            await using var fixture = await Fixture.CreateAsync();
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, fixture.App.Urls.Single() + "/sse");
            request.Headers.Accept.ParseAdd("text/event-stream");
            request.Content = new StringContent("{\"query\":\"subscription { nonexistent }\"}", Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync();
            Assert.Contains("event: next\ndata: {\"errors\":", text);
            Assert.EndsWith("event: complete\ndata: \n\n", text);
        }

        [Theory]
        [InlineData("Bearer test-token", false)]
        [InlineData(null, false)]
        [InlineData("Bearer test-token, Bearer second-token", false)]
        [InlineData("Bearer test-token, Bearer second-token", true)]
        public async Task SubscriptionFlowsThroughBridgeAndWorkerExecutorAsync(string? authorization, bool handshakeHeader)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            if (handshakeHeader) { socket.Options.SetRequestHeader("Authorization", authorization); }
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init", ["payload"] = !handshakeHeader && authorization is not null ? new JObject { ["Authorization"] = authorization } : new JObject() }, timeout.Token);
            Assert.Equal("connection_ack", (await ReadAsync(socket, timeout.Token)).Value<string>("type"));
            Assert.Equal(authorization ?? string.Empty, (await fixture.InitializationHeaders.Task.WaitAsync(timeout.Token))["Authorization"]);
            var conversation = Guid.NewGuid();
            await SendAsync(socket, new JObject
            {
                ["id"] = "one",
                ["type"] = "subscribe",
                ["payload"] = new JObject { ["query"] = $$"""subscription { conciergeMessageReceived(conversationId: "{{conversation}}") { response } }""" }
            }, timeout.Token);
            await fixture.SubscriptionCreated.Task.WaitAsync(timeout.Token);
            // Publication uses the same real GraphQL executor as the registered subscription.
            await fixture.Executor.ExecuteAsync(ConciergeTests.Mutation(conversation, "ping"));
            var reply = await ReadAsync(socket, timeout.Token);
            Assert.Equal("next", reply.Value<string>("type"));
            Assert.Equal("one", reply.Value<string>("id"));
            Assert.Equal("pong", (string?)reply["payload"]?["data"]?["conciergeMessageReceived"]?["response"]);
            Assert.Null(reply["payload"]?["data"]?["conciergeMessageReceived"]?["messageId"]);
            await SendAsync(socket, new JObject { ["type"] = "complete", ["id"] = "one" }, timeout.Token);
            await fixture.SubscriptionStopped.Task.WaitAsync(timeout.Token);
        }

        [Fact]
        public async Task AnonymousConnectionCanInitializeAsync()
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init" }, timeout.Token);
            Assert.Equal("connection_ack", (await ReadAsync(socket, timeout.Token)).Value<string>("type"));
        }

        [Fact]
        public async Task InvalidCredentialsCannotInitializeAsync()
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init", ["payload"] = new JObject { ["Authorization"] = "Bearer invalid" } }, timeout.Token);
            var frame = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), timeout.Token);
            Assert.Equal(4403, (int)frame.CloseStatus!);
            Assert.False(fixture.SubscriptionCreated.Task.IsCompleted);
        }

        [Theory]
        [InlineData("Authorization", "\"\"")]
        [InlineData("authorization", "\"   \"")]
        [InlineData("Authorization", "null")]
        [InlineData("Authorization", "123")]
        [InlineData("Authorization", "{}")]
        [InlineData("Authorization", "\"Bearer test-token,\"")]
        [InlineData("Authorization", "\"Bearer test-token, Bearer token\\r\\ninjected: value\"")]
        [InlineData("Authorization", "\"Bearer token\\r\\ninjected: value\"")]
        public async Task SuppliedInvalidInitializationCredentialCannotBecomeAnonymousAsync(string name, string value)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init", ["payload"] = new JObject { [name] = JToken.Parse(value) } }, timeout.Token);
            var frame = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), timeout.Token);
            Assert.Equal(4403, (int)frame.CloseStatus!);
            Assert.False(fixture.InitializationHeaders.Task.IsCompleted);
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("123")]
        [InlineData("\"invalid\"")]
        [InlineData("true")]
        public async Task NonObjectInitializationPayloadClosesWithProtocolErrorAsync(string payload)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init", ["payload"] = JToken.Parse(payload) }, timeout.Token);
            var frame = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), timeout.Token);
            Assert.Equal(4400, (int)frame.CloseStatus!);
            Assert.False(fixture.InitializationHeaders.Task.IsCompleted);
        }

        [Fact]
        public async Task EmptyHandshakeCredentialCannotBecomeAnonymousAsync()
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            socket.Options.SetRequestHeader("Authorization", " ");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init" }, timeout.Token);
            var frame = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), timeout.Token);
            Assert.Equal(4403, (int)frame.CloseStatus!);
            Assert.False(fixture.InitializationHeaders.Task.IsCompleted);
        }

        [Theory]
        [InlineData(WebSocketCloseStatus.NormalClosure)]
        [InlineData(WebSocketCloseStatus.EndpointUnavailable)]
        public async Task ClientCloseIsAcknowledgedAndSubscriptionIsStoppedAsync(WebSocketCloseStatus status)
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init" }, timeout.Token);
            Assert.Equal("connection_ack", (await ReadAsync(socket, timeout.Token)).Value<string>("type"));
            await SendAsync(socket, new JObject
            {
                ["id"] = "closing",
                ["type"] = "subscribe",
                ["payload"] = new JObject { ["query"] = $$"""subscription { conciergeMessageReceived(conversationId: "{{Guid.NewGuid()}}") { response } }""" }
            }, timeout.Token);
            await fixture.SubscriptionCreated.Task.WaitAsync(timeout.Token);
            // The callback can finish before the bridge receives its session ID.
            // A protocol pong confirms the bridge processed subscribe before closing.
            await SendAsync(socket, new JObject { ["type"] = "ping" }, timeout.Token);
            Assert.Equal("pong", (await ReadAsync(socket, timeout.Token)).Value<string>("type"));

            await socket.CloseOutputAsync(status, "Client finished", timeout.Token);
            var frame = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), timeout.Token);

            Assert.Equal(WebSocketMessageType.Close, frame.MessageType);
            Assert.Equal(status, frame.CloseStatus);
            Assert.Equal("Client finished", frame.CloseStatusDescription);
            await fixture.SubscriptionStopped.Task.WaitAsync(timeout.Token);
        }

        [Fact]
        public async Task BridgePreservesTracingHeadersThroughWorkerCallbackAsync()
        {
            await using var fixture = await Fixture.CreateAsync();
            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol(TransportOptions.Protocol);
            var headers = new Dictionary<string, string>
            {
                ["api-channel"] = "concierge-test",
                ["api-correlation-id"] = Guid.NewGuid().ToString(),
                ["api-session-id"] = Guid.NewGuid().ToString(),
                ["api-operation-id"] = Guid.NewGuid().ToString()
            };
            foreach (var header in headers)
            {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await socket.ConnectAsync(SocketUri(fixture.App), timeout.Token);
            await SendAsync(socket, new JObject { ["type"] = "connection_init" }, timeout.Token);
            Assert.Equal("connection_ack", (await ReadAsync(socket, timeout.Token)).Value<string>("type"));
            var received = await fixture.InitializationHeaders.Task.WaitAsync(timeout.Token);
            foreach (var header in headers)
            {
                Assert.Equal(header.Value, received[header.Key]);
            }
        }

        private static Uri SocketUri(WebApplication app) => new(app.Urls.Single().Replace("http:", "ws:") + "/socket");
        private static Task SendAsync(ClientWebSocket socket, JObject value, CancellationToken token) =>
            socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(value.ToString(Newtonsoft.Json.Formatting.None))), WebSocketMessageType.Text, true, token);
        private static async Task<JObject> ReadAsync(ClientWebSocket socket, CancellationToken token)
        {
            using var message = new MemoryStream();
            var bytes = new byte[4096];
            WebSocketReceiveResult frame;
            do
            {
                frame = await socket.ReceiveAsync(new ArraySegment<byte>(bytes), token);
                Assert.Equal(WebSocketMessageType.Text, frame.MessageType);
                message.Write(bytes, 0, frame.Count);
            } while (!frame.EndOfMessage);
            return JObject.Parse(Encoding.UTF8.GetString(message.ToArray()));
        }

        private sealed class Fixture : IAsyncDisposable
        {
            private const string Secret = "test-only-bridge-credential-32-characters";
            private readonly ServiceProvider provider;
            private readonly NativeSubscriptionRegistry registry;
            public WebApplication App { get; private set; } = null!;
            public IRequestExecutor Executor { get; private set; } = null!;
            public TaskCompletionSource SubscriptionCreated { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource SubscriptionStopped { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource<Dictionary<string, string>> InitializationHeaders { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

            private Fixture(ServiceProvider provider)
            {
                this.provider = provider;
                registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            }

            public static async Task<Fixture> CreateAsync()
            {
                var fixture = new Fixture(new ServiceCollection().AddConcierge().BuildServiceProvider());
                fixture.Executor = await fixture.provider.GetRequestExecutorAsync();
                await fixture.registry.StartAsync(CancellationToken.None);
                var builder = Builder(new TransportOptions { Path = "/socket", BridgeSecret = Secret });
                fixture.App = builder.Build();
                fixture.App.UseWebSockets();
                fixture.App.UseMiddleware<NativeWebSocketMiddleware>();
                fixture.App.UseMiddleware<NativeSseMiddleware>();
                fixture.App.UseRouting();
                fixture.App.MapPost(TransportOptions.BridgeRoute, async context =>
                {
                    // Test-only authenticated boundary. Production uses the real Functions authentication middleware.
                    if ((context.Request.Headers.Authorization.Count > 0 &&
                        context.Request.Headers.Authorization != "Bearer test-token" &&
                        context.Request.Headers.Authorization != "Bearer test-token, Bearer second-token") || context.Request.Headers["X-Native-WebSocket-Bridge"] != Secret)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }
                    var body = JObject.Parse(await new StreamReader(context.Request.Body).ReadToEndAsync());
                    var principal = context.Request.Headers.Authorization.Count > 0 ? NativeSubscriptionTests.Caller() : new System.Security.Claims.ClaimsPrincipal();
                    var connectionId = Guid.Parse(body.Value<string>("connectionId")!);
                    JObject reply;
                    switch (body.Value<string>("command"))
                    {
                        case "init":
                            fixture.InitializationHeaders.TrySetResult(new[] { "Authorization", "api-channel", "api-correlation-id", "api-session-id", "api-operation-id" }
                                .ToDictionary(name => name, name => context.Request.Headers[name].ToString()));
                            reply = new JObject { ["ok"] = true };
                            break;
                        case "start":
                            reply = await fixture.registry.StartAsync(body, principal, connectionId);
                            fixture.SubscriptionCreated.TrySetResult();
                            break;
                        case "read": reply = await fixture.registry.ReadAsync(Guid.Parse(body.Value<string>("sessionId")!), principal, context.RequestAborted, connectionId); break;
                        default:
                            await fixture.registry.StopAsync(Guid.Parse(body.Value<string>("sessionId")!), principal, connectionId);
                            fixture.SubscriptionStopped.TrySetResult();
                            reply = new JObject { ["ok"] = true };
                            break;
                    }
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(reply.ToString(Newtonsoft.Json.Formatting.None));
                });
                await fixture.App.StartAsync();
                return fixture;
            }

            private static WebApplicationBuilder Builder(TransportOptions options)
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls("http://127.0.0.1:0");
                builder.Services.AddSingleton(options);

                return builder;
            }

            public async ValueTask DisposeAsync()
            {
                await App.StopAsync();
                await registry.StopAsync(CancellationToken.None);
                await App.DisposeAsync();
                registry.Dispose();
                await provider.DisposeAsync();
            }
        }
    }
}
