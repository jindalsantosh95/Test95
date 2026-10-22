using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class NativeSubscriptionTests
    {
        private static readonly Guid ConnectionId = Guid.NewGuid();
        internal static ClaimsPrincipal Caller(string subject = "alice") =>
            new(new ClaimsIdentity([new Claim("iss", "https://issuer.test"), new Claim("sub", subject)], "test"));

        [Fact]
        public async Task AnonymousCallerCannotReceiveProtectedGraphQLFieldAsync()
        {
            var services = new ServiceCollection().AddConcierge();
            services.AddGraphQLServer().AddAuthorization().AddTypeExtension<ProtectedSubscription>();
            services.AddSingleton<HotChocolate.AspNetCore.Authorization.IAuthorizationHandler,
                VAA.EAPI.GraphQL.FunctionHost.Security.HasRoleAuthorizationHandler>();
            await using var provider = services.BuildServiceProvider();
            using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            var start = await registry.StartAsync(new JObject { ["query"] = "subscription { protectedMessage }" }, new ClaimsPrincipal(), ConnectionId);
            if (start["errors"] is not null)
            {
                Assert.Null(start["sessionId"]);
                return;
            }
            var session = Guid.Parse(start.Value<string>("sessionId")!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                var pending = registry.ReadAsync(session, new ClaimsPrincipal(), timeout.Token, ConnectionId);
                await provider.GetRequiredService<HotChocolate.Subscriptions.ITopicEventSender>()
                    .SendAsync("protected-message", "secret", timeout.Token);
                var reply = await pending;
                Assert.NotNull(reply["result"]?["errors"]);
                Assert.DoesNotContain("secret", reply.ToString());
            }
            finally
            {
                await registry.StopAsync(session, new ClaimsPrincipal(), ConnectionId);
            }
        }

        [HotChocolate.Types.ExtendObjectType("Subscription")]
        public sealed class ProtectedSubscription
        {
            [HotChocolate.AspNetCore.Authorization.Authorize]
            [HotChocolate.Types.Subscribe]
            [HotChocolate.Types.Topic("protected-message")]
            public string ProtectedMessage([HotChocolate.EventMessage] string message) => message;
        }
        [Fact]
        public async Task ExecutesRealSubscriptionSelectionAndRejectsAnotherCallerAsync()
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            var conversation = Guid.NewGuid();
            var start = await registry.StartAsync(new JObject
            {
                ["query"] = "subscription Observe($id: UUID!) { conciergeMessageReceived(conversationId: $id) { response } }",
                ["operationName"] = "Observe",
                ["variables"] = new JObject { ["id"] = conversation }
            }, Caller(), ConnectionId);
            Assert.Null(start["errors"]);
            var session = Guid.Parse(start.Value<string>("sessionId")!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.ReadAsync(session, Caller("bob"), timeout.Token, ConnectionId));
            var pending = registry.ReadAsync(session, Caller(), timeout.Token, ConnectionId);
            await executor.ExecuteAsync(ConciergeTests.Mutation(conversation, "ping"));
            var result = await pending;
            Assert.Equal("pong", (string?)result["result"]?["data"]?["conciergeMessageReceived"]?["response"]);
            Assert.Null(result["result"]?["data"]?["conciergeMessageReceived"]?["messageId"]);
            await registry.StopAsync(session, Caller(), ConnectionId);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.ReadAsync(session, Caller(), timeout.Token, ConnectionId));
        }

        [Theory]
        [InlineData("query { conciergePing(conversationId: \"11111111-1111-4111-8111-111111111111\") { response } }")]
        [InlineData("mutation { __typename }")]
        [InlineData("not a graphql document")]
        [InlineData("subscription { missingField }")]
        public async Task RejectsInvalidOrNonSubscriptionDocumentsAsync(string query)
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            var result = await registry.StartAsync(new JObject { ["query"] = query }, Caller(), ConnectionId);
            Assert.NotNull(result["errors"]);
            Assert.Null(result["sessionId"]);
        }

        [Fact]
        public async Task AnonymousSubscriptionCannotBeAccessedFromAnotherConnectionAsync()
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            var result = await registry.StartAsync(new JObject
            {
                ["query"] = "subscription { conciergeMessageReceived(conversationId: \"11111111-1111-4111-8111-111111111111\") { response } }"
            }, new ClaimsPrincipal(), ConnectionId);
            Assert.Null(result["errors"]);
            var session = Guid.Parse(result.Value<string>("sessionId")!);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.ReadAsync(session, new ClaimsPrincipal(), CancellationToken.None, Guid.NewGuid()));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => registry.StopAsync(session, new ClaimsPrincipal(), Guid.NewGuid()));
            await registry.StopAsync(session, new ClaimsPrincipal(), ConnectionId);
        }
        [Fact]
        public async Task StopCancelsPendingReadWithoutLeakingStreamAsync()
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            using var registry = new NativeSubscriptionRegistry(provider.GetRequiredService<IRequestExecutorResolver>());
            var start = await registry.StartAsync(new JObject
            {
                ["query"] = "subscription { conciergeMessageReceived(conversationId: \"11111111-1111-4111-8111-111111111111\") { response } }"
            }, Caller(), ConnectionId);
            var session = Guid.Parse(start.Value<string>("sessionId")!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var read = registry.ReadAsync(session, Caller(), timeout.Token, ConnectionId);
            await registry.StopAsync(session, Caller(), ConnectionId).WaitAsync(timeout.Token);
            try { await read.WaitAsync(timeout.Token); }
            catch (OperationCanceledException) { }
            Assert.True(read.IsCompleted);
        }
    }
}
