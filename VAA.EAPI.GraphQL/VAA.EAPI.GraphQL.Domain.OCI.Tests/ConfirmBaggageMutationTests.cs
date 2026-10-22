using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class ConfirmBaggageMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ConfirmBaggageMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IBaggageServiceProvider, BaggageServiceProvider>()
                .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<ITelemetryService, TelemetryService>()
                .AddSingleton<ConfirmBaggageMutation>()
                .BuildServiceProvider();
        }
        private (IBaggageServiceProvider baggageServiceProvider, ICheckInServiceProvider checkInServiceProvider, IBasketServiceProvider basketServiceProvider,  IPaymentServiceProvider paymentService, ITelemetryService telemetryService, ILogger<ConfirmBaggageMutation> logger) GetConstructorDependencies()
        {
            return (
                _serviceProvider.GetRequiredService<IBaggageServiceProvider>(),
                _serviceProvider.GetRequiredService<ICheckInServiceProvider>(),
                _serviceProvider.GetRequiredService<IBasketServiceProvider>(),
                _serviceProvider.GetRequiredService<IPaymentServiceProvider>(),
                _serviceProvider.GetRequiredService<ITelemetryService>(),
                _serviceProvider.GetRequiredService<ILogger<ConfirmBaggageMutation>>());
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBaggageServiceProviderIsNull()
        {
            // Arrange
            var (_, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, telemetryService, logger) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(null, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, telemetryService, logger));

            Assert.Equal("baggageServiceProvider", exception.ParamName);

        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            // Arrange
            var (baggageServiceProvider, _, basketServiceProvider, paymentServiceProvider, telemetryService, logger) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(baggageServiceProvider, null, basketServiceProvider, paymentServiceProvider, telemetryService, logger));

            Assert.Equal("checkInServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBasketServiceProviderIsNull()
        {
            // Arrange
            var (baggageServiceProvider, checkInServiceProvider, _, paymentServiceProvider, telemetryService, logger) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(baggageServiceProvider, checkInServiceProvider, null, paymentServiceProvider, telemetryService, logger));

            Assert.Equal("basketServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenPaymentServiceProviderIsNull()
        {
            // Arrange
            var (baggageServiceProvider, checkInServiceProvider, basketServiceProvider, _, telemetryService, logger) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(baggageServiceProvider, checkInServiceProvider, basketServiceProvider, null, telemetryService, logger));

            Assert.Equal("paymentServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryServiceIsNull()
        {
            // Arrange
            var (baggageServiceProvider, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, _, logger) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(baggageServiceProvider, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, null, logger));

            Assert.Equal("telemetryService", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            // Arrange
            var (baggageServiceProvider, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, telemetryService, _) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ConfirmBaggageMutation(baggageServiceProvider, checkInServiceProvider, basketServiceProvider, paymentServiceProvider, telemetryService, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task Mutation_ShouldReportError_WhenCheckInIdIsMissing()
        {
            var mutation = _serviceProvider.GetRequiredService<ConfirmBaggageMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.ConfirmBaggage(new Models.Baggage.ConfirmBaggageRequest { }, context);

            // Assert
            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.Contains("checkInId", resolverContext.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_For500Scenarios()
        {
            var mutation = _serviceProvider.GetRequiredService<ConfirmBaggageMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId( null, "FakeRequirementsId", true, "Save-Baggage-TravelPartyId-Error", true, "FAKPNR", "LHR",
                ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName");

            var result = await mutation.ConfirmBaggage(
                new Models.Baggage.ConfirmBaggageRequest { CheckInId = checkInId },
                context);

            // Assert
            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.StartsWith("Mandatory fields missing in the request: PaymentStatus", resolverContext.Errors[0].Message);
        }

        [Fact]
        public async Task ConfirmBaggageMutation_ShouldReturnCheckinID_ForGoodRequest()
        {
            var _mutation = _serviceProvider.GetService<ConfirmBaggageMutation>()!;
            var jwtSecurityTokenHandler = _serviceProvider.GetService<JwtSecurityTokenHandler>()!;
            _ = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", false, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef91", true);
            var result = await _mutation.ConfirmBaggage(new Models.Baggage.ConfirmBaggageRequest

            {
                CheckInId = checkInId,
                PaymentReferenceId = "TestPaymentRefId123",
                PaymentStatus = "Success"
            }, context);

            //Assert
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.Equal("John", result.PassengerBags.First().GivenNames);
            Assert.Equal("Doe", result.PassengerBags.First().Surname);


            Assert.Equal("GBP", result.CurrencyCode);
            Assert.Equal("LHR", result.OriginAirportCode);
            Assert.Equal("JFK", result.DestinationAirportCode);
            Assert.Equal("150.75", result.TotalPaidAmount);
        }

        [Fact]
        public async Task ConfirmBaggageMutation_ShouldReturnNULL_ForFailedStatus()
        {
            var _mutation = _serviceProvider.GetService<ConfirmBaggageMutation>()!;
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", false, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef91", true);

            var result = await _mutation.ConfirmBaggage(new Models.Baggage.ConfirmBaggageRequest
            {
                CheckInId = checkInId,
                PaymentReferenceId = "TestPaymentRefId123",
                PaymentStatus = "Failed"
            }, context);

            //Assert
            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.StartsWith("Looks like something needs sorting at the airport.", resolverContext.Errors[0].Message);
        }

        [Fact]
        public async Task ConfirmBaggageMutation_ShouldReturnError_ForNullSSCIResponse()
        {
            var _mutation = _serviceProvider.GetService<ConfirmBaggageMutation>()!;
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", false, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef91", true);

            var result = await _mutation.ConfirmBaggage(new Models.Baggage.ConfirmBaggageRequest
            {
                CheckInId = checkInId,
                PaymentReferenceId = "TestPaymentRefIdNullSSCIResponse",
                PaymentStatus = "Success"
            }, context);

            // Assert
            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.StartsWith("We’re unable to complete your baggage", resolverContext.Errors[0].Message);
        }

        [Fact]
        public async Task ConfirmBaggageMutation_ShouldReturnError_ForFailedSSCIResponse()
        {
            var _mutation = _serviceProvider.GetService<ConfirmBaggageMutation>()!;
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", false, "TEST02-Id-SSCI", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "Richard", "Branson", "387bf1d4-6aa8-4126-9548-1b882991ef91", true);

            var result = await _mutation.ConfirmBaggage(new Models.Baggage.ConfirmBaggageRequest
            {
                CheckInId = checkInId,
                PaymentReferenceId = "TestPaymentRefIdFailedSSCIResponse",
                PaymentStatus = "Success"
            }, context);

            // Assert
            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.StartsWith("We’re unable to complete your baggage", resolverContext.Errors[0].Message);
        }
    }
}
