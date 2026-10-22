using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public sealed class RulesEngineServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IRulesEngineService _rulesEngineService;

        public RulesEngineServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(Constants.Environment.RuleEngineUrl, "https://tempuri.org/inrule");
            ConfigReader.Set(Constants.Environment.RuleEngineAppId, "rules-engine-app-id");
            ConfigReader.Set(Constants.Environment.RuleEngineOcpSubscriptionId, "rules-engine-subscription-key");

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IRulesEngineService, RulesEngineService>()
                .BuildServiceProvider();

            _rulesEngineService = serviceProvider.GetRequiredService<IRulesEngineService>();
        }

        [Fact]
        public async Task EarnAirlineAccrualCalculatorAsync_ReturnsRulesEngineResponse()
        {
            var response = await _rulesEngineService.EarnAirlineAccrualCalculatorAsync(CreateRequest(), CancellationToken.None);

            var segment = Assert.Single(response.SegmentResponses);
            Assert.Equal(750, segment.PointsAward);
            Assert.Equal(25, segment.TierPointsAward);
            Assert.Equal("LHR", segment.Origin);
            Assert.Equal("JFK", segment.Destination);
        }

        private static EarnAirlineAccrualCalculatorRequest CreateRequest()
        {
            return new EarnAirlineAccrualCalculatorRequest
            {
                CustomerDetails = new[]
                {
                    new CustomerDetail
                    {
                        MemberNumber = "999000111",
                        TierCode = "Gold",
                        MemberStatus = "Active",
                        MemberType = "Individual",
                        CampaignOrPromotionCode = string.Empty,
                        CurrentTierStartDate = "2026-01-01"
                    }
                },
                FlightSegments = new[]
                {
                    new FlightSegment
                    {
                        OriginAirportCode = "LHR",
                        DestinationAirportCode = "JFK",
                        SegmentDistance = 3451,
                        OperatingCarrier = "VS",
                        MarketingCarrier = "VS",
                        OperatingFlightNumber = "003",
                        MarketingFlightNumber = "003",
                        ScheduledDepartureTime = "2026-02-01T10:30:00Z",
                        CabinClass = "Upper",
                        ClassOfService = "J",
                        OperatingBookingClass = "J",
                        MarketingBookingClass = "J"
                    }
                }
            };
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("test-bearer-token");
        }
    }
}
