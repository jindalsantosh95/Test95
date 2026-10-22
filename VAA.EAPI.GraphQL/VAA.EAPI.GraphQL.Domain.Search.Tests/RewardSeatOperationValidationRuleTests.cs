using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class RewardSeatOperationValidationRuleTests
    {
        [Theory]
        [InlineData("query { rewardSeatAvailability }")]
        [InlineData("query { ...R ...R } fragment R on Query { rewardSeatAvailability }")]
        [InlineData("query { rewardSeatAvailability status @skip(if: true) }")]
        public async Task AllowsRewardAsTheOnlyRootField(string query)
        {
            var (json, calls) = await ExecuteAsync(query);

            Assert.DoesNotContain("errors", json);
            Assert.Equal(1, calls);
        }

        [Theory]
        [InlineData("query { rewardSeatAvailability status }")]
        [InlineData("query { ...R } fragment R on Query { first: rewardSeatAvailability second: rewardSeatAvailability }")]
        [InlineData("query { ...R } fragment R on Query { rewardSeatAvailability ...S } fragment S on Query { status }")]
        [InlineData("query { rewardSeatAvailability ... @defer { status } }")]
        public async Task RejectsMixedOrDeferredRewardRootsBeforeAnyResolverRuns(string query)
        {
            var (json, calls) = await ExecuteAsync(query);

            Assert.Contains("REWARD_SEAT_OPERATION_LIMIT", json);
            Assert.Equal(0, calls);
        }

        [Fact]
        public async Task AllowsOrdinaryQueries()
        {
            var (json, calls) = await ExecuteAsync("query { status }");

            Assert.DoesNotContain("errors", json);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task LeavesMutationsUnchangedWhenRewardIsEnabled()
        {
            var (json, calls) = await ExecuteAsync("mutation { first: update second: update }");

            Assert.DoesNotContain("errors", json);
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task InspectionBudgetFailsBeforeResolvingAnOversizedOperation()
        {
            var query = "query { ...R } fragment R on Query { " +
                string.Join(" ", Enumerable.Range(0, 1025).Select(index => $"s{index}: status")) +
                " rewardSeatAvailability }";

            var (json, calls) = await ExecuteAsync(query);

            Assert.Contains("REWARD_SEAT_INSPECTION_LIMIT", json);
            Assert.Equal(0, calls);
        }

        [Fact]
        public async Task UnrelatedQueriesBypassTheRewardInspectionBudget()
        {
            var query = "query { ...S } fragment S on Query { " +
                string.Join(" ", Enumerable.Range(0, 1025).Select(index => $"s{index}: status")) + " }";

            var (json, calls) = await ExecuteAsync(query);

            Assert.DoesNotContain("errors", json);
            Assert.Equal(1025, calls);
        }

        private static async Task<(string Json, int Calls)> ExecuteAsync(string query)
        {
            var calls = 0;
            var flag = ConfigReader.Try(Constants.Environment.EnableRewardSeatAvailability);
            try
            {
                ConfigReader.Set(Constants.Environment.EnableRewardSeatAvailability, "true");
                var (provider, executor) = await RewardSeatGraphQlTestHost.CreateAsync(
                    configureServices: services => services.Replace(
                        ServiceDescriptor.Singleton<IRewardSeatAvailabilityQueryHandler>(
                            new CountingHandler(() => Interlocked.Increment(ref calls)))),
                    configureSchema: builder =>
                    {
                        builder.AddTypeExtension(new HotChocolate.Types.ObjectTypeExtension(descriptor =>
                        {
                            descriptor.Name("Query");
                            descriptor.Field("status").Type<HotChocolate.Types.StringType>()
                                .Resolve(_ => ValueTask.FromResult<object?>(Interlocked.Increment(ref calls).ToString()));
                        }));
                        builder.AddMutationType(descriptor => descriptor.Name("Mutation")
                            .Field("update").Type<HotChocolate.Types.StringType>()
                            .Resolve(_ => ValueTask.FromResult<object?>(Interlocked.Increment(ref calls).ToString())));
                    });
                using var ownedProvider = provider;
                query = query.Replace("rewardSeatAvailability",
                    "rewardSeatAvailability(request: { slice: { origin: \"LHR\", destination: \"JFK\", departure: \"2026-11-20\" }, years: [2026], months: [NOVEMBER] }) { month }",
                    StringComparison.Ordinal);
                var request = QueryRequestBuilder.New().SetQuery(query).Create();
                var result = await executor.ExecuteAsync(request);
                if (result is IResponseStream stream)
                {
                    var json = new StringBuilder();
                    await using (stream)
                    {
                        await foreach (var item in stream.ReadResultsAsync()) json.Append(item.ToJson());
                    }
                    return (json.ToString(), calls);
                }
                return (result.ToJson(), calls);
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.EnableRewardSeatAvailability, flag ?? string.Empty);
            }
        }

        private sealed class CountingHandler(Action onCall) : IRewardSeatAvailabilityQueryHandler
        {
            public Task<IEnumerable<RewardSeatAvailabilityMonthResponse>> HandleAsync(
                RewardSeatAvailabilityRequest request, CancellationToken cancellationToken)
            {
                onCall();
                return Task.FromResult<IEnumerable<RewardSeatAvailabilityMonthResponse>>([]);
            }
        }
    }
}
