using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SaveBaggageMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public SaveBaggageMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                .AddSingleton<IBaggageServiceProvider, BaggageServiceProvider>()
                .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
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
        public void Constructor_ShouldThrowNullException_WhenBaggageServiceProviderIsNull()
        {
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SaveBaggageMutation>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new SaveBaggageMutation(null, checkInServiceProvider, basketServiceProvider, telemetryService, logger));

            Assert.Equal("baggageServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            var baggageServiceProvider = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SaveBaggageMutation>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new SaveBaggageMutation(baggageServiceProvider, null, basketServiceProvider, telemetryService, logger));

            Assert.Equal("checkInServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBasketServiceProviderIsNull()
        {
            var baggageServiceProvider = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SaveBaggageMutation>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new SaveBaggageMutation(baggageServiceProvider, checkInServiceProvider, null, telemetryService, logger));

            Assert.Equal("basketServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryServiceIsNull()
        {
            var baggageServiceProvider = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<SaveBaggageMutation>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new SaveBaggageMutation(baggageServiceProvider, checkInServiceProvider, basketServiceProvider, null, logger));

            Assert.Equal("telemetryService", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            var baggageServiceProvider = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();

            var exception = Assert.Throws<ArgumentNullException>(() => new SaveBaggageMutation(baggageServiceProvider, checkInServiceProvider, basketServiceProvider, telemetryService, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullandReportError_WhenCheckInIdisNull()
        {
            var provider = _serviceProvider.GetRequiredService<SaveBaggageMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await provider.SaveBaggage(new Models.Baggage.SaveBaggageRequest { }, resolverProvider.Object);

            //Assert
            Assert.Null(result);

            resolverProvider.Verify(x => x.ReportError(
                It.Is<string>(static m =>
                    m.Is("Mandatory fields missing in the request: CheckInId"))),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_For500Scenarios()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "Save-Baggage-TravelPartyId-Error", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName");

            var provider = _serviceProvider.GetRequiredService<SaveBaggageMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => provider.SaveBaggage(new Models.Baggage.SaveBaggageRequest { CheckInId = checkInId }, resolverProvider.Object));
        }

        [Fact]
        public async Task Mutation_ShouldReturnCheckinID_ForGoodRequest()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var provider = _serviceProvider.GetRequiredService<SaveBaggageMutation>();

            var result = await provider.SaveBaggage(new Models.Baggage.SaveBaggageRequest
            {
                CheckInId = checkInId,
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
                        BaggageOfferId = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().Price.Amount
                    }
                ]
            }, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketId"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketIdExpiryTime"));
            Assert.True(DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload["basketIdExpiryTime"])) > DateTimeOffset.UtcNow);

        }

        [Fact]
        public async Task CreateBasketRequest_ShouldMapRequestedEmail_FromSaveBaggageRequest_WhenEmailProvided()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var request = new Models.Baggage.SaveBaggageRequest()
            {
                CheckInId = checkInId,
                EmailAddress = "provided@email.com",
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
                        BaggageOfferId = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().Price.Amount
                    }
                ]
            };

            var checkInProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();

            var checkInResponse = await checkInProvider.GetCheckInResponseAsync("TEST02-Id-SSCI");


            var result = (CreateBasketRequest)(request, checkInResponse, Enumerable.Empty<PassengerAllowance>());

            //Assert
            Assert.Equal("provided@email.com", result.ContactDetails.EmailAddress);
        }

        [Fact]
        public async Task CreateBasketRequest_ShouldMapRequestedEmail_FromSaveBaggageRequest_WhenCheckInResponseIsNull()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var request = new Models.Baggage.SaveBaggageRequest()
            {
                CheckInId = checkInId,
                EmailAddress = "provided@email.com",
                SelectedBaggageOffer =
                [
                    new SelectedBaggageOffer
                    {
                        PassengerNiN = baggageAllowance.Passengers.First().PassengerNiN,
                        BaggageOfferId = baggageAllowance.Passengers.First().BaggageOffers.First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.First().BaggageOffers.First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.First().BaggageOffers.First().Price.Amount
                    }
                ]
            };

            var result = (CreateBasketRequest)(request, null, Enumerable.Empty<PassengerAllowance>());

            //Assert
            Assert.Equal("provided@email.com", result.ContactDetails.EmailAddress);
        }

        [Fact]
        public async Task CreateBasketRequest_ShouldMapDefaultEmail_FromCheckInResponse_WhenEmailNotProvided()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var request = new Models.Baggage.SaveBaggageRequest()
            {
                CheckInId = checkInId,
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
                        BaggageOfferId = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().baggageOfferId,
                        BaggageCount = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().BaggageCount,
                        TotalPrice = baggageAllowance.Passengers.Skip(1).First().BaggageOffers.First().Price.Amount
                    }
                ]
            };

            var checkInProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();

            var checkInResponse = await checkInProvider.GetCheckInResponseAsync("TEST02-Id-SSCI");

            var result = (CreateBasketRequest)(request, checkInResponse, Enumerable.Empty<PassengerAllowance>());

            //Assert
            Assert.NotEmpty(result.ContactDetails.EmailAddress);
            Assert.Equal(checkInResponse.Passengers.FirstOrDefault()?.Email, result.ContactDetails.EmailAddress);

        }
    }
}
