using System.Collections.Specialized;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Stitch.Core.WebSockets;
using Xunit;

namespace VAA.EAPI.GraphQL.Stitch.Tests
{
    public sealed class RemoteSubscriptionTests
    {
        [Theory]
        [InlineData("https://example.com:8443/graphql/", "https://example.com:8443/graphql/sse")]
        [InlineData("http://localhost:7071/api/v1/GraphQL", "http://localhost:7071/api/v1/GraphQL/sse")]
        public void DerivesSseEndpointFromExistingHttpConfiguration(string http, string expected)
        {
            var endpoints = new RemoteSubscriptionEndpoints(new NameValueCollection { ["general"] = http });
            Assert.Equal(expected, endpoints.Get("general", true).AbsoluteUri);
        }

        [RedisFact]
        public async Task RedisBridgeReadsAndStopsStitchedSubscriptionFromAnotherWorker()
        {
            var prefix = "stitch-test-" + Guid.NewGuid().ToString("N");
            var configuration = Core.Helpers.ConfigReader.Get("RedisSubscriptionTestConnectionString");
            using var ownerStore = new VAA.EAPI.GraphQL.FunctionHost.WebSockets.RedisSubscriptionSessionStore(configuration, prefix);
            using var callbackStore = new VAA.EAPI.GraphQL.FunctionHost.WebSockets.RedisSubscriptionSessionStore(configuration, prefix);
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            using var owner = new VAA.EAPI.GraphQL.FunctionHost.WebSockets.NativeSubscriptionRegistry(
                provider.GetRequiredService<IRequestExecutorResolver>(), distributed: ownerStore);
            using var callback = new VAA.EAPI.GraphQL.FunctionHost.WebSockets.NativeSubscriptionRegistry(
                provider.GetRequiredService<IRequestExecutorResolver>(), distributed: callbackStore);
            await owner.StartAsync(CancellationToken.None);
            var connection = Guid.NewGuid();
            var start = await owner.StartAsync(new Newtonsoft.Json.Linq.JObject
            {
                ["query"] = "subscription { message(channel: \"redis-room\") }"
            }, null, connection);
            Assert.Null(start["errors"]);
            var id = Guid.Parse(start.Value<string>("sessionId")!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            try
            {
                await remote.Subscribed.Task.WaitAsync(timeout.Token);
                var executor = await provider.GetRequestExecutorAsync();
                await using var mutation = await executor.ExecuteAsync("mutation { post(channel: \"redis-room\", message: \"ping\") }", timeout.Token);
                Assert.Contains("pong", mutation.ToJson());
                Newtonsoft.Json.Linq.JObject result;
                do { result = await callback.ReadAsync(id, null, timeout.Token, connection); } while (!result.HasValues);
                Assert.Equal("pong", result["result"]?["data"]?["message"]?.ToString());
                await callback.StopAsync(id, null, connection);
                await remote.Disconnected.Task.WaitAsync(timeout.Token);
            }
            finally
            {
                try { await callback.StopAsync(id, null, connection); } catch (UnauthorizedAccessException) { }
                await owner.StopAsync(CancellationToken.None);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task StitchedMutationPublishesToRemoteSubscriptionWithAliasAndVariables(bool sse)
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            var executor = await provider.GetRequestExecutorAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var request = QueryRequestBuilder.New()
                .SetQuery("subscription Listen($channel: String!) { received: message(channel: $channel) }")
                .SetOperation("Listen").SetVariableValue("channel", "room-one")
                .SetGlobalState(RemoteSubscriptionEndpoints.HeadersProperty, new Dictionary<string, string> { ["Accept"] = sse ? "text/event-stream" : "application/json" }).Create();
            await using var result = await executor.ExecuteAsync(request, timeout.Token);
            Assert.IsAssignableFrom<IResponseStream>(result);
            await using var events = ((IResponseStream)result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await remote.Subscribed.Task.WaitAsync(timeout.Token);
            await using var mutation = await executor.ExecuteAsync("mutation { post(channel: \"room-one\", message: \"ping\") }", timeout.Token);
            Assert.Contains("pong", mutation.ToJson());
            Assert.True(await pending.WaitAsync(timeout.Token));
            Assert.Equal("pong", events.Current.Data!["received"]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task RoutesSubscriptionsByOwningSchemaAndForwardsCredentials(bool sse)
        {
            await using var first = await RemoteServer.StartAsync("first", "firstMessage");
            await using var second = await RemoteServer.StartAsync("second", "secondMessage");
            await using var provider = CreateStitch(first, second);
            var executor = await provider.GetRequestExecutorAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var request = QueryRequestBuilder.New().SetQuery("subscription { secondMessage(channel: \"two\") }")
                .SetGlobalState(RemoteSubscriptionEndpoints.HeadersProperty, new Dictionary<string, string>
                {
                    ["Accept"] = sse ? "text/event-stream" : "application/json",
                    ["Authorization"] = "Bearer synthetic-test-token",
                    ["Ocp-Apim-Subscription-Key"] = "synthetic-test-key"
                }).Create();
            await using var result = await executor.ExecuteAsync(request, timeout.Token);
            Assert.IsAssignableFrom<IResponseStream>(result);
            await using var events = ((IResponseStream)result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await second.Subscribed.Task.WaitAsync(timeout.Token);
            await second.App.Services.GetRequiredService<ITopicEventSender>().SendAsync("two", "second-event", timeout.Token);
            Assert.True(await pending.WaitAsync(timeout.Token));
            Assert.Equal("second-event", events.Current.Data!["secondMessage"]);
            Assert.False(first.Subscribed.Task.IsCompleted);
            Assert.Equal("Bearer synthetic-test-token", second.Authorization);
            Assert.Equal("synthetic-test-key", second.SubscriptionKey);
        }

        [Fact]
        public async Task MissingEndpointReturnsGraphQLErrorWithoutOpeningSocket()
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            var services = new ServiceCollection();
            services.AddSingleton(new RemoteSubscriptionEndpoints(new NameValueCollection()));
            services.AddHttpClient(remote.Name, c => c.BaseAddress = remote.Url);
            services.AddGraphQL().TryAddTypeInterceptor<RemoteSubscriptionTypeInterceptor>().AddRemoteSchema(remote.Name);
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            await using var result = await executor.ExecuteAsync("subscription { message(channel: \"room\") }");
            Assert.Contains("No safe subscription endpoint", result.ToJson());
            Assert.False(remote.Subscribed.Task.IsCompleted);
        }

        [Theory]
        [InlineData("https://example.com/dev/graphql/public", "wss://example.com/dev/graphql/public/ws")]
        [InlineData("https://example.com:8443/graphql/", "wss://example.com:8443/graphql/ws")]
        [InlineData("http://localhost:7071/api/v1/GraphQL", "ws://localhost:7071/api/v1/GraphQL/ws")]
        public void DerivesSocketEndpointFromHttpSchema(string source, string expected)
        {
            var endpoints = new RemoteSubscriptionEndpoints(
                new NameValueCollection { ["general"] = source });
            Assert.Equal(new Uri(expected), endpoints.Get("general"));
        }

        [Theory]
        [InlineData("http://example.com/graphql")]
        [InlineData("ws://example.com/graphql")]
        [InlineData("wss://example.com/graphql")]
        [InlineData("https://example.com/graphql?key=secret")]
        [InlineData("https://user:password@example.com/graphql")]
        [InlineData("https://example.com/graphql#fragment")]
        public void RejectsUnsafeDerivedEndpointWhenSubscribed(string source)
        {
            var endpoints = new RemoteSubscriptionEndpoints(
                new NameValueCollection { ["general"] = source });
            Assert.Throws<GraphQLException>(() => endpoints.Get("general"));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task StructuredEventsPreserveFragmentsAliasesAndScalarTypes(bool required, bool list)
        {
            await using var remote = await RemoteServer.StartAsync("general", "message", required, list);
            await using var provider = CreateStitch(remote);
            var executor = await provider.GetRequestExecutorAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var result = await executor.ExecuteAsync(
                "subscription { payload: details(channel: \"room\") { ...Fields } } fragment Fields on Message { renamed: text id sequence }", timeout.Token);
            Assert.IsAssignableFrom<IResponseStream>(result);
            await using var events = ((IResponseStream)result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await remote.Subscribed.Task.WaitAsync(timeout.Token);
            await remote.App.Services.GetRequiredService<ITopicEventSender>().SendAsync("room", "pong", timeout.Token);
            Assert.True(await pending.WaitAsync(timeout.Token));
            Assert.Null(events.Current.Errors);
            var json = events.Current.ToJson();
            Assert.Contains("\"renamed\":\"pong\"", json.Replace(" ", "").Replace("\r", "").Replace("\n", ""));
            Assert.Contains("12345678-1234-1234-1234-123456789abc", json);
        }

        [Fact]
        public async Task InvalidSubscriptionIsRejectedByStitchedSchema()
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            var executor = await provider.GetRequestExecutorAsync();
            await using var result = await executor.ExecuteAsync("subscription { message }");
            Assert.NotNull(Assert.IsAssignableFrom<IQueryResult>(result).Errors);
            Assert.False(remote.Subscribed.Task.IsCompleted);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task RegistryStopClosesDownstreamSubscription(bool sse)
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var registry = new VAA.EAPI.GraphQL.FunctionHost.WebSockets.NativeSubscriptionRegistry(
                provider.GetRequiredService<IRequestExecutorResolver>());
            var connection = Guid.NewGuid();
            var start = await registry.StartAsync(Newtonsoft.Json.Linq.JObject.Parse(
                "{\"query\":\"subscription { message(channel: \\\"room\\\") }\"}"), null, connection, new Dictionary<string, object>
                {
                    [RemoteSubscriptionEndpoints.HeadersProperty] = new Dictionary<string, string> { ["Accept"] = sse ? "text/event-stream" : "application/json" }
                });
            var id = Guid.Parse(start.Value<string>("sessionId")!);
            await remote.Subscribed.Task.WaitAsync(timeout.Token);
            await registry.StopAsync(id, null, connection).WaitAsync(timeout.Token);
            await remote.Disconnected.Task.WaitAsync(timeout.Token);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.ReadAsync(id, null, timeout.Token, connection));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DownstreamAuthenticationFailureReturnsError(bool sse)
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            var executor = await provider.GetRequestExecutorAsync();
            var request = QueryRequestBuilder.New().SetQuery("subscription { message(channel: \"room\") }")
                .SetGlobalState(RemoteSubscriptionEndpoints.HeadersProperty, new Dictionary<string, string> { ["Accept"] = sse ? "text/event-stream" : "application/json", ["Authorization"] = "Bearer invalid" }).Create();
            await using var result = await executor.ExecuteAsync(request);
            Assert.NotNull(Assert.IsAssignableFrom<IQueryResult>(result).Errors);
            Assert.False(remote.Subscribed.Task.IsCompleted);
        }

        [Fact]
        public async Task PreservesDownstreamErrorCodeAndAliasedPath()
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            await using var provider = CreateStitch(remote);
            var executor = await provider.GetRequestExecutorAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var result = await executor.ExecuteAsync("subscription { received: message(channel: \"room\") }", timeout.Token);
            await using var events = Assert.IsAssignableFrom<IResponseStream>(result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await remote.Subscribed.Task.WaitAsync(timeout.Token);
            await remote.App.Services.GetRequiredService<ITopicEventSender>().SendAsync("room", "error", timeout.Token);
            Assert.True(await pending.WaitAsync(timeout.Token));
            var error = Assert.Single(events.Current.Errors!);
            Assert.Equal("POC_ERROR", error.Code);
            Assert.Equal("received", error.Path!.ToList().Single());
        }

        [Fact]
        public async Task GeneratedConflictingFieldMappingRoutesWithVariables()
        {
            await using var first = await RemoteServer.StartAsync("first", "firstMessage");
            await using var second = await RemoteServer.StartAsync("second", "secondMessage");
            await using var provider = CreateStitch(first, second);
            var executor = await provider.GetRequestExecutorAsync();
            var field = executor.Schema.SubscriptionType!.Fields.Single(x => x.Name.Contains("second", StringComparison.Ordinal) && x.Name.Contains("details", StringComparison.Ordinal));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var request = QueryRequestBuilder.New().SetQuery($"subscription($room: String!) {{ event: {field.Name}(channel: $room) {{ text }} }}")
                .SetVariableValue("room", "two").Create();
            await using var result = await executor.ExecuteAsync(request, timeout.Token);
            await using var events = Assert.IsAssignableFrom<IResponseStream>(result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await second.Subscribed.Task.WaitAsync(timeout.Token);
            await second.App.Services.GetRequiredService<ITopicEventSender>().SendAsync("two", "mapped", timeout.Token);
            Assert.True(await pending.WaitAsync(timeout.Token));
            Assert.Null(events.Current.Errors);
            Assert.Contains("mapped", events.Current.ToJson());
            Assert.False(first.Subscribed.Task.IsCompleted);
        }

        public sealed record Message(string Text, Guid Id, int Sequence);
        public sealed record Room(string Channel);

        [Fact]
        public async Task RewritesRenamedInputVariableTypesForRemoteSchema()
        {
            await using var remote = await RemoteServer.StartAsync("general", "message");
            var services = new ServiceCollection();
            services.AddHttpClient(remote.Name, c => c.BaseAddress = remote.Url);
            services.AddSingleton(new RemoteSubscriptionEndpoints(
                new NameValueCollection { [remote.Name] = remote.Url.ToString() }));
            services.AddGraphQL().TryAddTypeInterceptor<RemoteSubscriptionTypeInterceptor>()
                .AddRemoteSchema(remote.Name).RenameType("RoomInput", "GatewayRoom", remote.Name);
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var request = QueryRequestBuilder.New().SetQuery("subscription($filter: GatewayRoom!) { filtered(filter: $filter) }")
                .SetVariableValue("filter", new Dictionary<string, object> { ["channel"] = "room" }).Create();
            await using var result = await executor.ExecuteAsync(request, timeout.Token);
            await using var events = Assert.IsAssignableFrom<IResponseStream>(result).ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var pending = events.MoveNextAsync().AsTask();
            await remote.Subscribed.Task.WaitAsync(timeout.Token);
            await remote.App.Services.GetRequiredService<ITopicEventSender>().SendAsync("room", "renamed", timeout.Token);
            Assert.True(await pending.WaitAsync(timeout.Token));
            Assert.Null(events.Current.Errors);
            Assert.Equal("renamed", events.Current.Data!["filtered"]);
        }

        private static ServiceProvider CreateStitch(params RemoteServer[] remotes)
        {
            var services = new ServiceCollection();
            var http = new NameValueCollection();
            foreach (var remote in remotes)
            {
                http[remote.Name] = remote.Url.ToString();
                services.AddHttpClient(remote.Name, c => c.BaseAddress = remote.Url);
            }
            services.AddSingleton(new RemoteSubscriptionEndpoints(http));
            var builder = services.AddGraphQL().TryAddTypeInterceptor<RemoteSubscriptionTypeInterceptor>();
            foreach (var remote in remotes) { builder.AddRemoteSchema(remote.Name); }
            return services.BuildServiceProvider();
        }

        private sealed class RemoteServer : IAsyncDisposable
        {
            public required WebApplication App { get; init; }
            public required string Name { get; init; }
            public Uri Url { get; private set; } = null!;
            public TaskCompletionSource Subscribed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource Disconnected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public string? Authorization { get; private set; }
            public string? SubscriptionKey { get; private set; }

            public static async Task<RemoteServer> StartAsync(string name, string field, bool requiredDetails = false, bool listDetails = false)
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls("http://127.0.0.1:0");
                builder.Logging.ClearProviders();
                RemoteServer server = null!;
                builder.Services.AddGraphQLServer().AddInMemorySubscriptions()
                    .AddQueryType(q => q.Name("Query").Field("ping").Resolve("pong"))
                    .AddMutationType(m => m.Name("Mutation").Field("post").Type<StringType>()
                        .Argument("channel", a => a.Type<NonNullType<StringType>>())
                        .Argument("message", a => a.Type<NonNullType<StringType>>())
                        .Resolve(async ctx =>
                        {
                            var response = ctx.ArgumentValue<string>("message") == "ping" ? "pong" : "ping";
                            await ctx.Service<ITopicEventSender>().SendAsync(ctx.ArgumentValue<string>("channel"), response);
                            return response;
                        }))
                    .AddSubscriptionType(s =>
                    {
                        s.Name("Subscription").Field(field).Type<StringType>()
                        .Argument("channel", a => a.Type<NonNullType<StringType>>())
                        .Subscribe(async ctx =>
                        {
                            var stream = await ctx.Service<ITopicEventReceiver>().SubscribeAsync<string>(ctx.ArgumentValue<string>("channel"));
                            server.Subscribed.TrySetResult();
                            return stream;
                        })
                        .Resolve(ctx => ctx.GetEventMessage<string>() == "error"
                            ? throw new GraphQLException(ErrorBuilder.New().SetMessage("Synthetic failure").SetCode("POC_ERROR").Build())
                            : ctx.GetEventMessage<string>());
                        var details = s.Field("details");
                        if (listDetails && requiredDetails) { details.Type<NonNullType<ListType<NonNullType<ObjectType<Message>>>>>(); }
                        else if (listDetails) { details.Type<ListType<ObjectType<Message>>>(); }
                        else if (requiredDetails) { details.Type<NonNullType<ObjectType<Message>>>(); }
                        else { details.Type<ObjectType<Message>>(); }
                        details.Argument("channel", a => a.Type<NonNullType<StringType>>())
                        .Subscribe(async ctx =>
                        {
                            var stream = await ctx.Service<ITopicEventReceiver>().SubscribeAsync<string>(ctx.ArgumentValue<string>("channel"));
                            server.Subscribed.TrySetResult();
                            return stream;
                        })
                        .Resolve(ctx =>
                        {
                            var message = new Message(ctx.GetEventMessage<string>(), Guid.Parse("12345678-1234-1234-1234-123456789abc"), 7);
                            return listDetails ? new[] { message } : (object)message;
                        });
                        s.Field("filtered").Type<StringType>()
                            .Argument("filter", a => a.Type<InputObjectType<Room>>())
                            .Subscribe(async ctx =>
                            {
                                var stream = await ctx.Service<ITopicEventReceiver>().SubscribeAsync<string>(ctx.ArgumentValue<Room>("filter").Channel);
                                server.Subscribed.TrySetResult();
                                return stream;
                            })
                            .Resolve(ctx => ctx.GetEventMessage<string>());
                    });
                var app = builder.Build();
                server = new RemoteServer { App = app, Name = name };
                app.UseWebSockets();
                app.Use(async (context, next) =>
                {
                    if (context.WebSockets.IsWebSocketRequest || context.Request.Path == "/graphql/sse")
                    {
                        server.Authorization = context.Request.Headers.Authorization.ToString();
                        server.SubscriptionKey = context.Request.Headers["Ocp-Apim-Subscription-Key"].ToString();
                        if (server.Authorization == "Bearer invalid")
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }
                    }
                    try { await next(context); }
                    finally
                    {
                        if (context.WebSockets.IsWebSocketRequest || context.Request.Path == "/graphql/sse") { server.Disconnected.TrySetResult(); }
                    }
                });
                app.MapGraphQL();
                app.MapGraphQL("/graphql/ws");
                app.MapGraphQL("/graphql/sse");
                await app.StartAsync();
                var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                server.Url = new Uri(address + "/graphql");
                return server;
            }

            public async ValueTask DisposeAsync() { await App.StopAsync(); await App.DisposeAsync(); }
        }
    }
}
