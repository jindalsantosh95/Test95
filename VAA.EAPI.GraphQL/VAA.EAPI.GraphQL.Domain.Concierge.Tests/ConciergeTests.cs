using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class ConciergeTests
    {
        [Fact]
        public async Task QueriesReturnFreshPingPongRepliesAsync()
        {
            var services = new ServiceCollection();
            services.AddConcierge();
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var id = Guid.NewGuid();
            var query = $$"""
                { conciergePing(conversationId: "{{id}}") { messageId response createdAtUtc }
                  conciergePong(conversationId: "{{id}}") { messageId response createdAtUtc } }
                """;
            var first = JObject.Parse((await executor.ExecuteAsync(query)).ToJson());
            var second = JObject.Parse((await executor.ExecuteAsync(query)).ToJson());
            Assert.Null(first["errors"]);
            Assert.Null(second["errors"]);
            Assert.Equal("pong", (string?)first["data"]?["conciergePing"]?["response"]);
            Assert.Equal("ping", (string?)first["data"]?["conciergePong"]?["response"]);
            Assert.NotEqual((string?)first["data"]?["conciergePing"]?["messageId"],
                (string?)second["data"]?["conciergePing"]?["messageId"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("oversize")]
        public async Task InvalidMessageReturns400Async(string message)
        {
            var services = new ServiceCollection();
            services.AddConcierge();
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var request = QueryRequestBuilder.New()
                .SetQuery("mutation($id: UUID!, $message: String!) { postConciergeMessage(conversationId: $id, message: $message) { messageId } }")
                .SetVariableValue("id", Guid.NewGuid())
                .SetVariableValue("message", message == "oversize" ? new string('x', 501) : message)
                .Create();
            var result = Assert.IsAssignableFrom<IQueryResult>(await executor.ExecuteAsync(request));
            Assert.Equal("400", Assert.Single(result.Errors!).Code);
        }

        [Fact]
        public async Task SubscriptionIsolatesConversationsAndDeliversMultipleMessagesAsync()
        {
            var services = new ServiceCollection();
            services.AddConcierge();
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var id = Guid.NewGuid();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var subscriptionResult = await executor.ExecuteAsync(
                $$"""subscription { conciergeMessageReceived(conversationId: "{{id}}") { messageId conversationId message response } }""",
                timeout.Token);
            Assert.True(subscriptionResult is IResponseStream, subscriptionResult is IQueryResult error ? error.ToJson() : "Expected a subscription stream.");
            await using var stream = (IResponseStream)subscriptionResult;
            await using var events = stream.ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
            var next = events.MoveNextAsync().AsTask();
            await executor.ExecuteAsync(Mutation(Guid.NewGuid(), "unrelated"), timeout.Token);
            var firstMutation = JObject.Parse((await executor.ExecuteAsync(Mutation(id, "ping"), timeout.Token)).ToJson());
            Assert.True(await next.WaitAsync(timeout.Token));
            var first = JObject.Parse(events.Current.ToJson());
            Assert.Equal("pong", (string?)first["data"]?["conciergeMessageReceived"]?["response"]);
            Assert.Equal((string?)firstMutation["data"]?["postConciergeMessage"]?["messageId"],
                (string?)first["data"]?["conciergeMessageReceived"]?["messageId"]);
            Assert.Equal(id, (Guid?)first["data"]?["conciergeMessageReceived"]?["conversationId"]);
            Assert.Equal(id, (Guid?)firstMutation["data"]?["postConciergeMessage"]?["conversationId"]);
            next = events.MoveNextAsync().AsTask();
            var secondMutation = JObject.Parse((await executor.ExecuteAsync(Mutation(id, "pong"), timeout.Token)).ToJson());
            Assert.True(await next.WaitAsync(timeout.Token));
            var second = JObject.Parse(events.Current.ToJson());
            Assert.Equal("ping", (string?)second["data"]?["conciergeMessageReceived"]?["response"]);
            Assert.Equal((string?)secondMutation["data"]?["postConciergeMessage"]?["messageId"],
                (string?)second["data"]?["conciergeMessageReceived"]?["messageId"]);
            Assert.Equal(id, (Guid?)second["data"]?["conciergeMessageReceived"]?["conversationId"]);
            Assert.Equal(id, (Guid?)secondMutation["data"]?["postConciergeMessage"]?["conversationId"]);
        }

        internal static string Mutation(Guid id, string message) =>
            $$"""mutation { postConciergeMessage(conversationId: "{{id}}", message: "{{message}}") { messageId conversationId response } }""";

        [Theory]
        [InlineData("query { conciergePing(conversationId: \"00000000-0000-0000-0000-000000000000\") { response } }")]
        [InlineData("mutation { postConciergeMessage(conversationId: \"00000000-0000-0000-0000-000000000000\", message: \"ping\") { response } }")]
        [InlineData("subscription { conciergeMessageReceived(conversationId: \"00000000-0000-0000-0000-000000000000\") { response } }")]
        public async Task EmptyConversationIsRejectedAsync(string operation)
        {
            var services = new ServiceCollection();
            services.AddConcierge();
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var executionResult = await executor.ExecuteAsync(operation);
            if (executionResult is IResponseStream stream)
            {
                await using (stream)
                {
                    var subscription = Assert.IsAssignableFrom<ISubscriptionResult>(stream);
                    Assert.Equal("400", Assert.Single(subscription.Errors!).Code);
                }
            }
            else
            {
                var result = Assert.IsAssignableFrom<IQueryResult>(executionResult);
                Assert.Equal("400", Assert.Single(result.Errors!).Code);
            }
        }

        [Fact]
        public async Task OrdinaryMessageReturnsDummyEchoAsync()
        {
            var services = new ServiceCollection();
            services.AddConcierge();
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var result = JObject.Parse((await executor.ExecuteAsync(Mutation(Guid.NewGuid(), "hello"))).ToJson());
            Assert.Null(result["errors"]);
            Assert.Equal("Concierge received: hello", (string?)result["data"]?["postConciergeMessage"]?["response"]);
        }
    }
}
