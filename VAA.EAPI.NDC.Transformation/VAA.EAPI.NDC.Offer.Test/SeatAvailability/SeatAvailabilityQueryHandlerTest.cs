using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.Delta.SeatAvailability;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using ErrorType = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS.ErrorType;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class SeatAvailabilityQueryHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public SeatAvailabilityQueryHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<SeatAvailabilityQueryHandler>();
                })
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddTransient<IGenericDataMapper, GenericDataMapper>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddTransient<ISeatAvailabilityValidations<IATA_SeatAvailabilityRS>, SeatAvailabilityValidations<IATA_SeatAvailabilityRS>>()
                .AddSingleton<SeatAvailabilityQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<SeatAvailabilityQueryHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task MapToErrorSeatAvailabilityRs_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            request.DistributionChain.First().ParticipatingOrg = null;
            var subscriptionKey = "1234";

            //Act
            var _handler = _serviceProvider.GetService<SeatAvailabilityQueryHandler>();
            var result = await _handler.Handle(new SeatAvailabilityQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task MapToSeatAvailabilityRs_NotFound_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var obj= new SeatAvailabilityQuery( request, null );

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validaton");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            //Act
            var _handler = _serviceProvider.GetService<SeatAvailabilityQueryHandler>();
            var result = await _handler.Handle(obj, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task SeatAvailabilityHandler_Response_for_PostsaleFlow()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PostFlowSeatAvailabilityRQ");
            var obj = new SeatAvailabilityQuery(request, null);

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validaton");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            //Act
            var _handler = _serviceProvider.GetService<SeatAvailabilityQueryHandler>();
            var result = await _handler.Handle(obj, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
        }

        [Fact]
        public void Validate_ErrorFor_Null_ParticipatingOrgObject()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].ParticipatingOrg = null;

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_Empty_ParticipatingOrgId()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].ParticipatingOrg.OrgID = string.Empty;
            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesAgentIdOfSalesAgent()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].SalesAgent = new() { SalesAgentID = string.Empty };

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesBranchIdOfSalesBranch()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].SalesBranch = new() { SalesBranchID = string.Empty };

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count() > 0);
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptyOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = string.Empty;

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count() > 0);
            Assert.Equal("912_7", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_ZeroOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "0";
            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count() > 0);
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_SellerOrdinalNotStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "2";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count() > 0);
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_NoErrorFor_SellerOrdinalStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "123";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.Empty(validationErrors);
        }

        [Fact]
        public void Validate_FlightSegmentsListInSeatRequest()
        {
            //Arrange
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");

            //Act
            var _handler = _serviceProvider.GetService<SeatAvailabilityQueryHandler>();
            var flightSegments = _handler.GetFlightSegments(offerDoc);

            //Assert
            Assert.NotNull(flightSegments);
            Assert.Equal(4, flightSegments.Count());
            Assert.Contains(flightSegments, x => x.OriginAirportCode == "LHR" && x.DestinationAirportCode == "JFK" && x.MarketingFlightNumber == "3");
            Assert.Equal("K", flightSegments.First(x => x.OriginAirportCode == "LHR" && x.DestinationAirportCode == "JFK" && x.MarketingFlightNumber == "3").ClassOfService);
            Assert.True(flightSegments.All(x => x.ClassOfService == "K"));
        }
    }
}
