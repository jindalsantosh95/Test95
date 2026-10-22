using VAA.EAPI.GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class RedisFactAttribute : FactAttribute
    {
        public RedisFactAttribute()
        {
            if (ConfigReader.Try("RedisSubscriptionTestConnectionString").IsNullOrWhiteSpace())
            {
                Skip = "Set RedisSubscriptionTestConnectionString to run isolated Redis integration tests.";
            }
        }
    }

    public sealed class RedisSubscriptionScalingTests
    {
        private static string Prefix() => "eapi-test-" + Guid.NewGuid().ToString("N");
        private static RedisSubscriptionSessionStore Store(string prefix) =>
            new(ConfigReader.Get("RedisSubscriptionTestConnectionString"), prefix);

        [Fact]
        public void InitialRedisOutageDoesNotPoisonSingletonConnection()
        {
            // Reserve an unavailable local endpoint so no external Redis is needed.
            using var endpoint = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            endpoint.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 0));
            var port = ((System.Net.IPEndPoint)endpoint.LocalEndPoint!).Port;
            using var store = new RedisSubscriptionSessionStore($"127.0.0.1:{port},connectTimeout=100,connectRetry=0,abortConnect=true", Prefix());

            var connection = store.Connection;

            Assert.False(connection.IsConnected);
            Assert.Same(connection, store.Connection);
        }

        [RedisFact]
        public async Task MutationOnAnotherInstanceDeliversToSubscriptionAndCallbackOnThirdWorker()
        {
            var prefix = Prefix();
            using var first = Store(prefix);
            using var second = Store(prefix);
            using var third = Store(prefix);
            await using var owner = Node(first);
            await using var publisher = Node(second);
            using var registry = new NativeSubscriptionRegistry(owner.GetRequiredService<IRequestExecutorResolver>(), distributed: first);
            using var callback = new NativeSubscriptionRegistry(publisher.GetRequiredService<IRequestExecutorResolver>(), distributed: third);
            await registry.StartAsync(CancellationToken.None);
            var connection = Guid.NewGuid();
            var conversation = Guid.NewGuid();
            var caller = new ClaimsPrincipal();
            var started = await registry.StartAsync(new JObject
            {
                ["query"] = "subscription { conciergeMessageReceived(conversationId: \"" + conversation + "\") { messageId response } }"
            }, caller, connection);
            Assert.Null(started["errors"]);
            var id = Guid.Parse(started.Value<string>("sessionId")!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => callback.ReadAsync(id, caller, timeout.Token, Guid.NewGuid()));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => callback.StopAsync(id, NativeSubscriptionTests.Caller("other"), connection));
                var executor = await publisher.GetRequestExecutorAsync();
                var result = await executor.ExecuteAsync("mutation { postConciergeMessage(conversationId: \"" + conversation + "\", message: \"ping\") { messageId response } }", timeout.Token);
                var mutation = JObject.Parse(result.ToJson());
                Assert.Null(mutation["errors"]);
                JObject received;
                do { received = await callback.ReadAsync(id, caller, timeout.Token, connection); }
                while (!received.HasValues);
                Assert.Equal("pong", received["result"]?["data"]?["conciergeMessageReceived"]?["response"]?.Value<string>());
                Assert.Equal(mutation["data"]?["postConciergeMessage"]?["messageId"]?.Value<string>(),
                    received["result"]?["data"]?["conciergeMessageReceived"]?["messageId"]?.Value<string>());
                await callback.StopAsync(id, caller, connection);
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.ReadAsync(id, caller, timeout.Token, connection));
            }
            finally
            {
                try { await first.StopAsync(id, NativeSubscriptionRegistry.Owner(caller), connection); }
                catch (UnauthorizedAccessException) { }
                await registry.StopAsync(CancellationToken.None);
            }
        }

        [RedisFact]
        public async Task MailboxIsIsolatedByCallerConnectionAndNamespaceAndStopCannotResurrectIt()
        {
            using var store = Store(Prefix());
            using var otherNamespace = Store(Prefix());
            var id = Guid.NewGuid();
            var connection = Guid.NewGuid();
            await store.CreateAsync(id, "owner", connection);
            try
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => store.ReadAsync(id, "other", connection, CancellationToken.None));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => otherNamespace.ReadAsync(id, "owner", connection, CancellationToken.None));
                await store.StopAsync(id, "owner", connection);
                Assert.False(await store.PublishAsync(id, "owner", connection, new JObject { ["result"] = "late" }));
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => store.ReadAsync(id, "owner", connection, CancellationToken.None));
            }
            finally
            {
                try { await store.StopAsync(id, "owner", connection); } catch (UnauthorizedAccessException) { }
            }
        }

        [RedisFact]
        public async Task BufferOverflowTerminatesSessionRatherThanDroppingEventsSilently()
        {
            using var store = Store(Prefix());
            var id = Guid.NewGuid();
            var connection = Guid.NewGuid();
            await store.CreateAsync(id, "owner", connection);
            try
            {
                var oversized = new JObject { ["result"] = new string('x', 1048577) };
                Assert.False(await store.PublishAsync(id, "owner", connection, oversized));
                var result = await store.ReadAsync(id, "owner", connection, CancellationToken.None);
                Assert.Contains("buffer exceeded", result.ToString());
                Assert.True((await store.ReadAsync(id, "owner", connection, CancellationToken.None)).Value<bool>("complete"));
            }
            finally { await store.StopAsync(id, "owner", connection); }
        }

        [RedisFact]
        public async Task LostOwnerAndExpiredSessionFailClosed()
        {
            var prefix = Prefix();
            using var store = Store(prefix);
            var id = Guid.NewGuid();
            var connection = Guid.NewGuid();
            await store.CreateAsync(id, "owner", connection);
            // Only this test's random key is altered. Never scan, flush, or alter existing application data.
            var key = prefix + ":session:{" + id.ToString("N") + "}:meta";
            var db = store.Connection.GetDatabase();
            await db.HashSetAsync(key, "heartbeat", 0);
            var result = await store.ReadAsync(id, "owner", connection, CancellationToken.None);
            Assert.Contains("worker unavailable", result.ToString());
            await store.CreateAsync(id, "owner", connection);
            await db.KeyExpireAsync(key, TimeSpan.Zero);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => store.ReadAsync(id, "owner", connection, CancellationToken.None));
        }

        [RedisFact]
        public async Task ConcurrentCallbacksConsumeEachResultOnlyOnce()
        {
            var prefix = Prefix();
            using var first = Store(prefix);
            using var second = Store(prefix);
            var id = Guid.NewGuid();
            var connection = Guid.NewGuid();
            await first.CreateAsync(id, "owner", connection);
            try
            {
                await first.PublishAsync(id, "owner", connection, new JObject { ["result"] = "one" });
                await first.PublishAsync(id, "owner", connection, new JObject { ["result"] = "two" }, complete: true);
                var results = await Task.WhenAll(first.ReadAsync(id, "owner", connection, CancellationToken.None),
                    second.ReadAsync(id, "owner", connection, CancellationToken.None));
                Assert.Equal(new[] { "one", "two" }, results.Select(value => value.Value<string>("result")).OrderBy(value => value));
            }
            finally { await first.StopAsync(id, "owner", connection); }
        }

        private static ServiceProvider Node(RedisSubscriptionSessionStore store)
        {
            var services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            var builder = services.AddGraphQLServer();
            startup.RegisterQueries(builder, false);
            startup.RegisterMutations(builder, false);
            startup.RegisterSubscriptions(builder, false);
            builder.AddQueryType(q => q.Name("Query")).AddMutationType(q => q.Name("Mutation"))
                .AddSubscriptionType(q => q.Name("Subscription")).AddRedisSubscriptions(_ => store.Connection);
            return services.BuildServiceProvider();
        }
    }
}
