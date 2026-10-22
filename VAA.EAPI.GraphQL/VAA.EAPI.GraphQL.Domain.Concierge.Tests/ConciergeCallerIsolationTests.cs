using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class ConciergeCallerIsolationTests
    {
        [Fact]
        public async Task ValidatedCallersOnlyReceiveTheirOwnEventsOnSameConversationAsync()
        {
            await using var provider = new ServiceCollection().AddConcierge().BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();
            var conversation = Guid.NewGuid();
            var previous = AsyncContext.Current;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                SetCaller("alice");
                await using var alice = Assert.IsAssignableFrom<IResponseStream>(await executor.ExecuteAsync(
                    $$"""subscription { conciergeMessageReceived(conversationId: "{{conversation}}") { messageId message } }""", timeout.Token));
                await using var aliceEvents = alice.ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
                var aliceNext = aliceEvents.MoveNextAsync().AsTask();

                SetCaller("bob");
                await using var bob = Assert.IsAssignableFrom<IResponseStream>(await executor.ExecuteAsync(
                    $$"""subscription { conciergeMessageReceived(conversationId: "{{conversation}}") { messageId message } }""", timeout.Token));
                await using var bobEvents = bob.ReadResultsAsync().GetAsyncEnumerator(timeout.Token);
                var bobNext = bobEvents.MoveNextAsync().AsTask();

                // Publish non-matching events first. A shared anonymous topic would deliver these instead.
                AsyncContext.Current = null;
                await executor.ExecuteAsync(ConciergeTests.Mutation(conversation, "anonymous"), timeout.Token);
                SetCaller("bob");
                var bobMutation = JObject.Parse((await executor.ExecuteAsync(ConciergeTests.Mutation(conversation, "bob"), timeout.Token)).ToJson());
                Assert.True(await bobNext.WaitAsync(timeout.Token));
                var bobEvent = JObject.Parse(bobEvents.Current.ToJson());
                Assert.Equal("bob", (string?)bobEvent["data"]?["conciergeMessageReceived"]?["message"]);
                Assert.Equal((string?)bobMutation["data"]?["postConciergeMessage"]?["messageId"], (string?)bobEvent["data"]?["conciergeMessageReceived"]?["messageId"]);

                SetCaller("alice");
                var aliceMutation = JObject.Parse((await executor.ExecuteAsync(ConciergeTests.Mutation(conversation, "alice"), timeout.Token)).ToJson());
                Assert.True(await aliceNext.WaitAsync(timeout.Token));
                var aliceEvent = JObject.Parse(aliceEvents.Current.ToJson());
                Assert.Equal("alice", (string?)aliceEvent["data"]?["conciergeMessageReceived"]?["message"]);
                Assert.Equal((string?)aliceMutation["data"]?["postConciergeMessage"]?["messageId"], (string?)aliceEvent["data"]?["conciergeMessageReceived"]?["messageId"]);
            }
            finally { AsyncContext.Current = previous; }
        }

        private static void SetCaller(string subject)
        {
            var context = new TestFunctionContext();
            context.Features.Set(new JwtPrincipalFeature(NativeSubscriptionTests.Caller(subject), new AuthToken()));
            AsyncContext.Current = new AsyncContext { Context = context };
        }
    }
}
