using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class PaymentEligibilityCheckMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        
        public PaymentEligibilityCheckMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                .AddSingleton<IBaggageServiceProvider, BaggageServiceProvider>()
                .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
                .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<ITelemetryService, TelemetryService>()
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddLogging()
                .AddSingleton(new Mock<IResolverContext>())
                .AddSingleton<CheckInQuery>()
                .AddSingleton<BaggageQuery>()
                .AddSingleton<SaveBaggageMutation>()
                .AddSingleton<PaymentEligibilityCheckMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBaggageServiceProviderIsNull()
        {
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();    

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(null, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, referenceServiceProvider, telemetryService, logger));

            Assert.Equal("baggageServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, null, basketServiceProvider, paymentServiceProvider, referenceServiceProvider, telemetryService, logger));

            Assert.Equal("checkInServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBasketServiceProviderIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, checkInServiceProvider, null, paymentServiceProvider, referenceServiceProvider, telemetryService, logger));

            Assert.Equal("basketServiceProvider", exception.ParamName);
        }


        [Fact]
        public void Constructor_ShouldThrowNullException_WhenPaymentServiceProviderIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, checkInServiceProvider, basketServiceProvider, null, referenceServiceProvider, telemetryService, logger));

            Assert.Equal("paymentServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenReferenceServiceProviderIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, null, telemetryService, logger));

            Assert.Equal("referenceServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryServiceIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<PaymentEligibilityCheckMutation>>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, referenceServiceProvider, null, logger));

            Assert.Equal("telemetryService", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            var baggageServiceProvide = _serviceProvider.GetRequiredService<IBaggageServiceProvider>();
            var basketServiceProvider = _serviceProvider.GetRequiredService<IBasketServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var checkInServiceProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var paymentServiceProvider = _serviceProvider.GetRequiredService<IPaymentServiceProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new PaymentEligibilityCheckMutation(baggageServiceProvide, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, referenceServiceProvider, telemetryService, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullandReportError_WhenCheckInIdisNull()
        {
            var paymentcheckProvider = _serviceProvider.GetRequiredService<PaymentEligibilityCheckMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                paymentcheckProvider.PaymentCheck(new PaymentCheckRequest { CheckInId = null }, resolverProvider.Object));
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullandThrowError_For500Scenarios()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "Payment-Check-Baggage-TravelPartyId-Error", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName", "387bf1d4-6aa8-4126-9548-1b882991ef90", true);

            var paymentcheckProvider = _serviceProvider.GetRequiredService<PaymentEligibilityCheckMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await paymentcheckProvider.PaymentCheck(new PaymentCheckRequest { CheckInId = checkInId }, resolverProvider.Object);

            //Assert
            Assert.Null(result);

            resolverProvider.Verify(x => x.ReportError(
                It.Is<IError>(static e =>
                    e.Message.StartsWith("We’re unable to complete your baggage process online."))),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldReturnCheckinID_ForGoodRequest()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson");

            var baggageQueryProvider = _serviceProvider.GetRequiredService<BaggageQuery>();

            var saveBaggageProvider = _serviceProvider.GetRequiredService<SaveBaggageMutation>();

            var paymentcheckProvider = _serviceProvider.GetRequiredService<PaymentEligibilityCheckMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var baggageAllowance = await baggageQueryProvider.GetBaggageAllowance(checkInId, false, resolverProvider.Object);

            var saveBaggageResponse = await saveBaggageProvider.SaveBaggage(new Models.Baggage.SaveBaggageRequest
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

            var result =  await paymentcheckProvider.PaymentCheck(new PaymentCheckRequest { CheckInId = saveBaggageResponse.CheckInId }, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketId"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(result.CheckInId).Payload.ContainsKey("basketIdExpiryTime"));
            Assert.False(result.PaymentPending);
            Assert.Null(result.PaymentUrl);
            Assert.Null(result.HPPLink);
            Assert.Null(result.PaymentReferenceId);
            Assert.Null(result.Amount.Currency);
            Assert.Equal(0, result.Amount.TotalAmount);
        }

        [Fact]
        public async Task Mutation_ShouldReturnPaymetUrlandHppLink_ForPaymentRequest()
        {
            //AsyncContext.Current.Context..Add("X-Forwarded-For", "203.0.113.5");

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "TEST02-Id-SSCI_payment", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef91", true);

            var paymentcheckProvider = _serviceProvider.GetRequiredService<PaymentEligibilityCheckMutation>();

            var resolverProvider = _serviceProvider.GetRequiredService<Mock<IResolverContext>>();

            var result = await paymentcheckProvider.PaymentCheck(new PaymentCheckRequest { CheckInId = checkInId }, resolverProvider.Object);

            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();

            //Assert
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.True(result.PaymentPending);
            Assert.NotNull(result.PaymentUrl);
            Assert.Contains("DFMOBILEOCI", result.HPPLink);
            Assert.NotNull(result.Amount);
            Assert.Equal("GBP", result.Amount.Currency);
            Assert.True(result.Amount.TotalAmount > 0);
            Assert.Equal("A15AATESTPRI2AF1458", result.PaymentReferenceId);
        }
    }
}
