using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class BaggageQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public BaggageQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                .AddSingleton<IAnalyticsService, AnalyticsService>()
                .AddSingleton<IBaggageServiceProvider, BaggageServiceProvider>()
                .AddSingleton<ILogAnalytics, LogAnalytics>()
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<ITelemetryService, TelemetryService>()
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddLogging()
                .AddSingleton(new Mock<IResolverContext>())
                .AddSingleton<CheckInQuery>()
                .AddSingleton<BaggageQuery>()
                .AddSingleton<SaveBaggageMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryServiceProvider = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BaggageQuery>>();
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(baggageServiceProvide, null, telemetryServiceProvider, tokenB2CProvider, basketServiceProvider, logger));

            Assert.Equal("checkInServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBaggageServiceProviderIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryServiceProvider = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BaggageQuery>>();
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(null, checkInServiceProvider, telemetryServiceProvider, tokenB2CProvider, basketServiceProvider, logger));

            Assert.Equal("baggageServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryServiceIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BaggageQuery>>();
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(baggageServiceProvide, checkInServiceProvider, null, tokenB2CProvider, basketServiceProvider, logger));

            Assert.Equal("telemetryServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProviderIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryServiceProvider = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BaggageQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(baggageServiceProvide, checkInServiceProvider, telemetryServiceProvider, null, basketServiceProvider, logger));

            Assert.Equal("loginTokenProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBasketServiceProviderIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var telemetryServiceProvider = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<BaggageQuery>>();
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(baggageServiceProvide, checkInServiceProvider, telemetryServiceProvider, tokenB2CProvider, null, logger));
            
            Assert.Equal("basketServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryServiceProvider = _serviceProvider.GetRequiredService<ITelemetryService>();
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new BaggageQuery(baggageServiceProvide, checkInServiceProvider, telemetryServiceProvider, tokenB2CProvider, basketServiceProvider, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task Query_ShouldReturnNullAndReportError_WhenCheckInIdisNull()
        {
            var provider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await provider.GetBaggageAllowance(null, false, resolverProvider.Object);

            //Assert
            Assert.Null(result);

            resolverProvider.Verify(x => x.ReportError(
                It.Is<string>(static m =>
                    m.Is("Mandatory fields missing in the request: checkInId"))),
                Times.Once);
        }

        [Fact]
        public async Task Query_ShouldReturnResponse_ForGoodRequest()
        {
            //Act
            var checkInProvider = _serviceProvider.GetRequiredService<CheckInQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var checkInResponse = await checkInProvider.InitiateCheckIn("TEST02", null, "LHR", "LAMONT", "PAIVA", resolverProvider.Object);

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var result = await baggageProvider.GetBaggageAllowance(checkInResponse.CheckInId, false, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert

            Assert.NotNull(result.Passengers);
            Assert.True(result.ShowBaggageAllowance);
            Assert.Null(result.EmailAddress);   // passenger not logged in and basket id isn't present in checkin token

            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("travelPartyId"));
            Assert.Equal(3, result.Passengers.Count());
            Assert.Equal(("Richard", "Branson"), (result.Passengers.First().GivenNames, result.Passengers.First().Surname));
            Assert.Equal("01.01", result.Passengers.First().PassengerNiN);
            Assert.Equal(("Rich", "Bran"), (result.Passengers.Skip(1).First().GivenNames, result.Passengers.Skip(1).First().Surname));
            Assert.Equal(7, result.Passengers.First().BaggageOffers.Count());
            Assert.Equal(65, result.Passengers.First().BaggageOffers.First().Price.Amount);
            Assert.Equal(65, result.Passengers.First().BaggageOffers.First().Price.AdditionalBagPrice);
            Assert.Equal(130, result.Passengers.First().BaggageOffers.Skip(1).First().Price.Amount);
            Assert.Equal(65, result.Passengers.First().BaggageOffers.Skip(1).First().Price.AdditionalBagPrice);
            Assert.Equal("GBP", result.Passengers.First().BaggageOffers.First().Price.Currency);
            Assert.Equal(830, result.Passengers.First().BaggageOffers.Last().Price.Amount);
            Assert.Null(result.Passengers.First().BaggageAdvisory);

            //AllAboutBags
            Assert.NotNull(result.AllAboutBags);
            Assert.Equal(2, result.AllAboutBags.EveryoneGets.Count());
            Assert.Equal("https://Test.Baggage.virginatlantic.com/", result.AllAboutBags.BaggageUrl);
            Assert.Null(result.BaggageLearnMoreUrl);
            Assert.Equal(IconType.carry_on, result.AllAboutBags.EveryoneGets.First().IconType);
            Assert.Equal("1 x 10kg carry-on bag", result.AllAboutBags.EveryoneGets.First().Title);
            Assert.True(result.AllAboutBags.EveryoneGets.First().Description.Count() == 2);
            Assert.Equal("Max dimensions: 23 x 36 x 56cm", result.AllAboutBags.EveryoneGets.First().Description.Skip(1).First());

            //Infant passenger
            var infantPassenger = result.Passengers.FirstOrDefault(x => x.IsInfantPassenger);
            Assert.NotNull(infantPassenger);
            Assert.Equal("01.03", infantPassenger.PassengerNiN);
            Assert.False(infantPassenger.BaggageOffers.Any());
            Assert.NotNull(infantPassenger.BaggageAdvisory);
            Assert.Equal("Infant bag allowance", infantPassenger.BaggageAdvisory.Title);
            Assert.StartsWith("Infants travelling on a lap", infantPassenger.BaggageAdvisory.Message);
        }

        [Fact]
        public async Task Query_ShouldReturnShowAllowanceFalse_ForPnrWithBaggageUnavailable()
        {
            //Act
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, null, false, travelPartyId: "TEST02-Id-SSCI-NotAvailale", resetTraParIdExpiryTime: true);

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result.ShowBaggageAllowance);
            Assert.Null(result.Passengers);
            Assert.False(result.ShowBaggageAllowance);
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
        }

        [Fact]
        public async Task Query_ShouldReturnEmailAddress_ForLoggedInPassenger()
        {
            var checkInProvider = _serviceProvider.GetRequiredService<CheckInQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var checkInResponse = await checkInProvider.InitiateCheckIn("TEST02", null, "LHR", "LAMONT", "PAIVA", resolverProvider.Object);

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            var token = JsonDocument.Parse(loginToken).RootElement.GetProperty("Token").GetString();

            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test-user")], "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = token };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var result = await baggageProvider.GetBaggageAllowance(checkInResponse.CheckInId, false, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result);
            Assert.False(jwtSecurityTokenHandler.ReadJwtToken(checkInResponse.CheckInId).Payload.ContainsKey("basketId"));
            Assert.False(result.EmailAddress.IsNullOrEmpty());
            Assert.IsType<string>(result.EmailAddress);
            Assert.Equal("TestThisEmail@example.com", result.EmailAddress);
        }


        [Fact]
        public async Task Query_ShouldReturnEmailAddress_FromBasketResponse_EvenWhenPassengerIsLoggedIn()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef90", true);

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            var token = JsonDocument.Parse(loginToken).RootElement.GetProperty("Token").GetString();

            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test-user")], "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = token };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result);
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketId"));
            Assert.False(result.EmailAddress.IsNullOrEmpty());
            Assert.Equal("TEST@TESTHISBAGS.COM", result.EmailAddress);
        }


        [Fact]
        public async Task Query_ShouldReturnEmailAddressAndNumberOfSelectedBags_FromBasket_WhenAddedToCheckInId()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var saveBaggageProvider = _serviceProvider.GetRequiredService<SaveBaggageMutation>();

            var saveBaggageRequest = new SaveBaggageRequest
            {
                CheckInId = baggageAllowance.CheckInId,
                SelectedBaggageOffer =
                [
                    new SelectedBaggageOffer
                    {
                        PassengerNiN = baggageAllowance.Passengers.First().PassengerNiN,
                        BaggageOfferId = baggageAllowance.Passengers.First().BaggageOffers.First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.First().BaggageOffers.First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.First().BaggageOffers.First().Price.Amount
                    },
                    new SelectedBaggageOffer
                    {
                        PassengerNiN = baggageAllowance.Passengers.Skip(1).First().PassengerNiN,
                        BaggageOfferId = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.Skip(1).First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.Skip(1).First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.Skip(1).First().Price.Amount
                    }
                ]
            };

            var saveBaggageResponse = await saveBaggageProvider.SaveBaggage(saveBaggageRequest, resolverProvider.Object);

            var result = await baggageProvider.GetBaggageAllowance(saveBaggageResponse.CheckInId, false, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result);
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketId"));
            Assert.False(result.EmailAddress.IsNullOrEmpty());
            Assert.Equal("TEST@TESTHISBAGS.COM", result.EmailAddress);
            Assert.Equal(3, result.Passengers.Count());

            //passenger 1
            var passenger1 = result.Passengers.First();
            Assert.True(saveBaggageRequest.SelectedBaggageOffer.First(x => x.PassengerNiN.Is(passenger1.PassengerNiN)).BaggageCount == passenger1.NumSelectedBags);

            //passenger 2
            var passenger2 = result.Passengers.Skip(1).First();
            Assert.True(saveBaggageRequest.SelectedBaggageOffer.First(x => x.PassengerNiN.Is(passenger2.PassengerNiN)).BaggageCount == passenger2.NumSelectedBags);

            //passenger 2   no bags selected
            var passenger3 = result.Passengers.Skip(2).First();
            Assert.False(saveBaggageRequest.SelectedBaggageOffer?.Any(x => x.PassengerNiN.Is(passenger3.PassengerNiN)));
            Assert.Equal(0, passenger3.NumSelectedBags);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}