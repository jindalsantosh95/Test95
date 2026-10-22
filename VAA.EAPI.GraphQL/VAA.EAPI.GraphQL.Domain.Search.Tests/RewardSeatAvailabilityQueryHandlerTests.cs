using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Caching;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{

    [Collection("EnvironmentDependentTests")]
    public sealed class RewardSeatAvailabilityQueryHandlerTests : IDisposable
    {
        private ServiceProvider? _provider;
        private readonly Dictionary<string, (string? ConfigValue, string? ProcessValue)> _previousSettings =
            new(StringComparer.OrdinalIgnoreCase);

        public void Dispose()
        {
            _provider?.Dispose();
            foreach (var (key, previous) in _previousSettings)
            {
                ConfigReader.Set(key, previous.ConfigValue ?? string.Empty);
                Environment.SetEnvironmentVariable(key, previous.ProcessValue, EnvironmentVariableTarget.Process);
            }
        }

        [Fact]
        public async Task HandleAsync_RejectsUnsupportedDirectedRouteWithoutCallingOffer()
        {
            var handler = Create("unsupported", out var factory);

            var error = await Assert.ThrowsAsync<GraphQLException>(() => handler.HandleAsync(Request(), CancellationToken.None));

            Assert.Contains(error.Errors, item => item.Code == "REWARD_SEAT_UNSUPPORTED_ROUTE");
            Assert.Empty(factory.GetResponses("https://reward-offer.test"));
        }

        [Theory]
        [InlineData("malformed")]
        [InlineData("enabled-missing-code")]
        public async Task HandleAsync_RejectsMalformedRouteSnapshotAsDependencyFailure(string route)
        {
            var handler = Create(route, out var factory);

            var error = await Assert.ThrowsAsync<GraphQLException>(() => handler.HandleAsync(Request(), CancellationToken.None));

            Assert.Contains(error.Errors, item => item.Code == "REWARD_SEAT_UNAVAILABLE");
            Assert.Empty(factory.GetResponses("https://reward-offer.test"));
        }

        [Fact]
        public async Task HandleAsync_RejectsInsecureReferenceEndpointBeforeAttachingSubscriptionKey()
        {
            var handler = Create("supported", out var factory, referenceTemplate: "http://reward-reference.test/{0}");

            var error = await Assert.ThrowsAsync<GraphQLException>(() => handler.HandleAsync(Request(), CancellationToken.None));

            Assert.Contains(error.Errors, item => item.Code == "REWARD_SEAT_UNAVAILABLE");
            Assert.Empty(factory.GetResponses("https://reward-offer.test"));
        }

        [Fact]
        public async Task HandleAsync_ReturnsOneMonthAndReusesCachedCompleteEmptyWorkUnits()
        {
            var handler = Create("supported", out var factory);

            var first = await handler.HandleAsync(Request(), CancellationToken.None);
            var callsAfterFirst = factory.GetResponses("https://reward-offer.test").Count;
            var second = await handler.HandleAsync(Request(), CancellationToken.None);

            var month = Assert.Single(first);
            Assert.Single(second);
            Assert.NotEmpty(month.PointsDays);
            Assert.All(month.PointsDays, day => Assert.Null(day.Seats));
            Assert.Equal(0, month.TotalAwardsSeatsForMonth);
            Assert.True(callsAfterFirst > 0);
            Assert.Equal(callsAfterFirst, factory.GetResponses("https://reward-offer.test").Count);
        }

        [Fact]
        public async Task HandleAsync_RevalidatesRouteOnEachRequest()
        {
            var handler = Create("supported", out var factory);
            Assert.Single(await handler.HandleAsync(Request(), CancellationToken.None));
            var routeCalls = factory.GetResponses("https://reward-routes.test").Count;
            Assert.Single(await handler.HandleAsync(Request(), CancellationToken.None));
            Assert.True(factory.GetResponses("https://reward-routes.test").Count > routeCalls);
        }

        [Fact]
        public async Task GraphQlOperation_ExecutesThroughTheHandlerAndMockedHttpBoundary()
        {
            var handler = Create("supported", out var factory);
            var request = Request();
            var previousFlag = ConfigReader.Try(Constants.Environment.EnableRewardSeatAvailability);
            try
            {
                ConfigReader.Set(Constants.Environment.EnableRewardSeatAvailability, "true");
                var (provider, executor) = await RewardSeatGraphQlTestHost.CreateAsync(
                    configureServices: services => services.Replace(
                        ServiceDescriptor.Singleton<IRewardSeatAvailabilityQueryHandler>(handler)));
                using (provider)
                {
                    var date = request.Slice.Departure;
                    var result = await executor.ExecuteAsync($$"""
                query {
                  rewardSeatAvailability(request: {
                    slice: { origin: "LHR", destination: "JFK", departure: "{{date:yyyy-MM-dd}}" }
                    years: [{{date.Year}}]
                    months: [{{((RewardSeatMonth)date.Month).ToString().ToUpperInvariant()}}]
                  }) { month totalAwardsSeatsForMonth }
                }
                """);

                    Assert.True(!result.ToJson().Contains("errors", StringComparison.Ordinal), result.ToJson());
                    Assert.NotEmpty(factory.GetResponses("https://reward-routes.test"));
                    Assert.NotEmpty(factory.GetResponses("https://reward-reference.test"));
                    Assert.NotEmpty(factory.GetResponses("https://reward-offer.test"));
                }
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.EnableRewardSeatAvailability, previousFlag ?? string.Empty);
            }
        }

        [Theory]
        [InlineData("2027-03-31T23:30:00Z", 2027, 4, 1, 2)]
        [InlineData("2027-03-28T23:30:00Z", 2027, 3, 29, 30)]
        [InlineData("2028-02-28T23:30:00Z", 2028, 2, 28, 29)]
        public async Task HandleAsync_ClipsOfferWorkUnitsByOriginLocalDateAcrossUtcDstAndLeapBoundaries(
            string instant, int year, int month, int firstDay, int lastDay)
        {
            var clock = new FixedTimeProvider(DateTimeOffset.Parse(instant, CultureInfo.InvariantCulture));
            var request = Request(year: year, month: month);
            var handler = Create("supported", out var factory, clock: clock, maxAdvanceDays: 1,
                endpoint: "https://reward-offer-boundary.test/api/internal/reward-seat-calendar-availability");

            var response = Assert.Single(await handler.HandleAsync(request, CancellationToken.None));

            var expectedStart = new DateOnly(year, month, firstDay);
            var expectedEnd = new DateOnly(year, month, lastDay);
            Assert.Equal(expectedStart, response.PointsDays.First().Date);
            Assert.Equal(expectedEnd, response.PointsDays.Last().Date);
            // Each checked-in mock matches the complete interval and a single anchor.
            // An incorrect interval/anchor reaches the harness's terminating handler.
            var observed = factory.GetResponses("https://reward-offer-boundary.test");
            Assert.Equal(new[] { expectedStart, expectedEnd }, observed
                .Select(item => DateOnly.ParseExact(Assert.Single(item.Headers.GetValues("X-Reward-Anchor")),
                    "yyyy-MM-dd", CultureInfo.InvariantCulture))
                .OrderBy(date => date));
        }

        [Fact]
        public async Task HandleAsync_PropagatesCancellationWithoutCallingOffer()
        {
            var handler = Create("supported", out var factory);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                handler.HandleAsync(Request(), cancellation.Token));

            Assert.Empty(factory.GetResponses("https://reward-offer.test"));
        }

        [Theory]
        [InlineData("https://reward-offer.test/api/internal/reward-seat-calendar-availability", null)]
        [InlineData("https://reward-offer.test/dev/OfferMicroservice/internal/reward-seat-calendar-availability", "offer-test-key")]
        public async Task HandleAsync_CallsOfferWithoutBearerOrIdentitySettings(string endpoint, string? subscriptionKey)
        {
            var handler = Create("supported", out var factory, endpoint: endpoint, offerSubscriptionKey: subscriptionKey);

            Assert.Single(await handler.HandleAsync(Request(), CancellationToken.None));

            var responses = factory.GetResponses("https://reward-offer.test");
            Assert.NotEmpty(responses);
            foreach (var response in responses)
            {
                Assert.NotNull(response.RequestMessage);
                Assert.Null(response.RequestMessage.Headers.Authorization);
                if (subscriptionKey is null)
                    Assert.False(response.RequestMessage.Headers.Contains("Ocp-Apim-Subscription-Key"));
                else
                    Assert.Equal(subscriptionKey, Assert.Single(
                        response.RequestMessage.Headers.GetValues("Ocp-Apim-Subscription-Key")));
            }
        }

        [Fact]
        public async Task HandleAsync_AllowsHttpForTheLocalOfferFunction()
        {
            var handler = Create("supported", out var factory,
                endpoint: "http://localhost:7071/api/reward-seat-calendar-availability");

            Assert.Single(await handler.HandleAsync(Request(), CancellationToken.None));

            Assert.NotEmpty(factory.GetResponses("http://localhost:7071"));
        }

        [Fact]
        public async Task HandleAsync_RejectsHttpForRemoteOfferEvenWhenItWouldReturnValidAvailability()
        {
            var handler = Create("supported", out var factory,
                endpoint: "http://reward-offer.test/api/internal/reward-seat-calendar-availability");

            var error = await Assert.ThrowsAsync<GraphQLException>(() => handler.HandleAsync(Request(), CancellationToken.None));

            Assert.Contains(error.Errors, item => item.Code == "REWARD_SEAT_UNAVAILABLE");
            Assert.Empty(factory.GetResponses("http://reward-offer.test"));
        }

        private RewardSeatAvailabilityQueryHandler Create(string route, out HttpClientFactory factory,
            string endpoint = "https://reward-offer.test/api/internal/reward-seat-calendar-availability",
            TimeProvider? clock = null,
            string referenceTemplate = "https://reward-reference.test/{0}",
            int maxAdvanceDays = 366, string? offerSubscriptionKey = "offer-test-key")
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            var values = new Dictionary<string, string?>
            {
                ["RewardSeatAvailability:MaxAdvanceDays"] = maxAdvanceDays.ToString(CultureInfo.InvariantCulture),
                ["RewardSeatAvailability:MaxWorkUnits"] = "8",
                ["RewardSeatAvailability:MaxConcurrency"] = "2",
                ["RewardSeatAvailability:OverallTimeoutSeconds"] = "30",
                ["RewardSeatAvailability:FareFamilyMappingVersion"] = "v1",
                ["RewardSeatAvailability:OfferCacheTtl"] = "00:15:00",
                ["RewardSeatAvailability:ItaVersion"] = "161",
                ["RewardSeatAvailability:SeatCaps:awardEconomy:Daily"] = "9",
                ["RewardSeatAvailability:SeatCaps:awardEconomy:Monthly"] = "100",
                ["RewardSeatAvailability:SeatCaps:awardPremium:Daily"] = "9",
                ["RewardSeatAvailability:SeatCaps:awardPremium:Monthly"] = "50",
                ["RewardSeatAvailability:SeatCaps:awardUpperClass:Daily"] = "9",
                ["RewardSeatAvailability:SeatCaps:awardUpperClass:Monthly"] = "50",
                ["Search:OfferMS:OCPKey"] = offerSubscriptionKey,
                ["RewardSeatAvailability:OfferCalendarEndpoint"] = endpoint,
                ["AirportInfoQuery:RoutesApi"] = "https://reward-routes.test/" + route,
                ["RefDataMS:GenericUrl"] = referenceTemplate,
                ["RefDataMS:OcpSubID"] = "test-key"
            };
            foreach (var (key, value) in values)
            {
                _previousSettings[key] = (
                    ConfigReader.Try(key),
                    Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process));
                ConfigReader.Set(key, value ?? string.Empty);
            }

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IDistributedCache, DistributedCache>();
            services.AddSingleton(clock ?? TimeProvider.System);
            services.AddSingleton<System.Net.Http.IHttpClientFactory, HttpClientFactory>();
            services.AddSingleton<IHttpConnector, HttpConnector>();
            services.AddSingleton<RewardSeatAvailabilityQueryHandler>();
            _provider = services.BuildServiceProvider();
            factory = (HttpClientFactory)_provider.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            return _provider.GetRequiredService<RewardSeatAvailabilityQueryHandler>();
        }

        private static RewardSeatAvailabilityRequest Request(int? year = null, int? month = null)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1);
            date = new DateOnly(date.Year, date.Month, 1);
            if (year is not null && month is not null) date = new DateOnly(year.Value, month.Value, 1);
            return new RewardSeatAvailabilityRequest
            {
                Slice = new RewardSeatSliceInput { Origin = "LHR", Destination = "JFK", Departure = date },
                Years = [date.Year],
                Months = [(RewardSeatMonth)date.Month]
            };
        }

        private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => now;
        }
    }
}
