using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Helper;
using Xunit;
using SearchEnvironment = VAA.EAPI.GraphQL.Domain.Search.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    [Collection("EnvironmentDependentTests")]
    public sealed class RewardSeatAvailabilityFeatureTests
    {
        [Theory]
        [InlineData("true", false, true)]
        [InlineData(" TRUE ", false, true)]
        [InlineData("true", true, false)]
        [InlineData("false", false, false)]
        [InlineData("yes", false, false)]
        public async Task RegisterQueries_RegistersRewardSeatAvailabilityOnlyForTheInsecureEnabledSchema(
            string flag,
            bool isSecure,
            bool expectedField)
        {
            var previousFlag = ConfigReader.Try(SearchEnvironment.EnableRewardSeatAvailability);

            try
            {
                ConfigReader.Set(SearchEnvironment.EnableRewardSeatAvailability, flag);
                var (serviceProvider, executor) = await RewardSeatGraphQlTestHost.CreateAsync(isSecure);
                using var provider = serviceProvider;
                Assert.Equal(expectedField, executor.Schema.QueryType.Fields.Any(field => field.Name == "rewardSeatAvailability"));
            }
            finally
            {
                ConfigReader.Set(SearchEnvironment.EnableRewardSeatAvailability, previousFlag ?? string.Empty);
            }
        }

        [Fact]
        public async Task RegisterQueries_ExposesTheRewardContractAndRejectsMultipleRewardRoots()
        {
            var previousFlag = ConfigReader.Try(SearchEnvironment.EnableRewardSeatAvailability);
            try
            {
                ConfigReader.Set(SearchEnvironment.EnableRewardSeatAvailability, "true");
                var (serviceProvider, executor) = await RewardSeatGraphQlTestHost.CreateAsync();
                using var provider = serviceProvider;
                var result = await executor.ExecuteAsync("query($request: RewardSeatAvailabilityRequestInput!) { " +
                    "first: rewardSeatAvailability(request: $request) { month } " +
                    "second: rewardSeatAvailability(request: $request) { month } }");
                Assert.Contains("REWARD_SEAT_OPERATION_LIMIT", result.ToJson());
                foreach (var typeName in new[]
                {
                    "RewardSeatAvailabilityMonth", "RewardSeatAvailabilityDay", "AvailabilitySeats", "RewardSeatCabin"
                })
                    Assert.NotNull(executor.Schema.GetType<ObjectType>(typeName));

                Assert.Equal(new[]
                {
                    "JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST",
                    "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER"
                }, executor.Schema.GetType<EnumType>("RewardSeatMonth").Values.Select(value => value.Name.ToString()));

                var sdl = executor.Schema.ToString();
                Assert.Contains("rewardSeatAvailability(request: RewardSeatAvailabilityRequestInput!): [RewardSeatAvailabilityMonth!]", sdl);
                Assert.DoesNotContain("RewardSeatPassenger", sdl);
                Assert.Contains("slice: RewardSeatSliceInput!", sdl);
                Assert.Contains("years: [Int!]!", sdl);
                Assert.Contains("months: [RewardSeatMonth!]!", sdl);
                Assert.DoesNotContain("permittedCarriers:", sdl);
                Assert.DoesNotContain("market:", sdl);
                Assert.Contains("pointsDays: [RewardSeatAvailabilityDay!]!", sdl);
                Assert.Contains("minPrice: Decimal", sdl);
                Assert.Contains("minAwardPointsTotal: Decimal!", sdl);
                Assert.Contains("seats: AvailabilitySeats", sdl);
                Assert.Contains("cabinPointsValue: Decimal!", sdl);
                Assert.Contains("isSaverAward: Boolean", sdl);
            }
            finally
            {
                ConfigReader.Set(SearchEnvironment.EnableRewardSeatAvailability, previousFlag ?? string.Empty);
            }
        }

    }
}
