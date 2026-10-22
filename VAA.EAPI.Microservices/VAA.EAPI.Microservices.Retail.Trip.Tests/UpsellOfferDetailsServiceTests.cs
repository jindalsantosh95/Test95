using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class UpsellOfferDetailsServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IUpsellOfferDetailsService _mockUpsellOfferDetailsService;
        private readonly IDeltaUpsellOfferDetailsService _mockDeltaUpsellOfferDetailsService;
        private readonly IDeltaUpsellOfferDetailsPrivateService _mockDeltaUpsellOfferDetailsPrivateService;
        private readonly ILogger<DeltaUpsellOfferDetailsService> _loggerDeltaUpsellOffer;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly ILogger<UpsellOfferDetailsService> _loggerUpsellOfferDetailsService;

        public UpsellOfferDetailsServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _air4TokenGenerator = new Air4TokenGenerator(_httpConnector);
            _mockDeltaUpsellOfferDetailsPrivateService = new DeltaUpsellOfferDetailsPrivateService(_httpConnector, _air4TokenGenerator);
            _loggerDeltaUpsellOffer = Microsoft.Extensions.Logging.Abstractions.NullLogger<DeltaUpsellOfferDetailsService>.Instance;
            _mockDeltaUpsellOfferDetailsService = new DeltaUpsellOfferDetailsService(_httpConnector, _loggerDeltaUpsellOffer);
            _loggerUpsellOfferDetailsService = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpsellOfferDetailsService>.Instance;
            _mockUpsellOfferDetailsService = new UpsellOfferDetailsService(_mockDeltaUpsellOfferDetailsService, _mockDeltaUpsellOfferDetailsPrivateService, _loggerUpsellOfferDetailsService);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            TripMSEnvironmentSetup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task RetrieveUpsellOfferDetails_ShouldSucceed()
        {
            // Arrange
            var tripBookingId = "2b724508-7a8f-4e68-b1dc-bcebd7cf174f";

            //Act
            var response = await _mockUpsellOfferDetailsService.GetUpsellOfferDetails(tripBookingId);

            //Assert
            Assert.True(response.IsSuccess == true);
            Assert.True(response.Value.Value.BookingId == tripBookingId);
        }

        [Fact]
        public async Task RetrieveUpsellOfferDetails_UsingPrivateEndpoints_ShouldSucceed()
        {
            // Arrange
            var tripBookingId = "2b724508-7a8f-4e68-b1dc-bcebd7cf174f";

            //Act
            var response = await _mockDeltaUpsellOfferDetailsPrivateService.RetrieveUpsellOfferDetails(tripBookingId);

            //Assert
            Assert.True(response != null);
            Assert.True(response.ReservationResourceId == tripBookingId);
        }

        [Fact]
        public async Task RetrieveUpsellOfferDetails_ExpiredTripId_ShouldErrorOut()
        {
            // Arrange
            var tripBookingId = "3b724508-7a8f-4e68-b1dc-bcebd7cf1767";

            //Act
            var response = await _mockUpsellOfferDetailsService.GetUpsellOfferDetails(tripBookingId);

            //Assert
            Assert.True(response.IsSuccess);
            Assert.True(response.Value.Errors.Any());
            Assert.Equal("NGTUPSL500", response.Value.Errors.First().Code);
        }

        [Fact]
        public async Task RetrieveUpsellOfferDetails_UsingprivateEndpoint_ExpiredTripId_ShouldErrorOut()
        {
            // Arrange
            var tripBookingId = "3b724508-7a8f-4e68-b1dc-bcebd7cf1767";


            try
            {
                //Act
                var response = await _mockDeltaUpsellOfferDetailsPrivateService.RetrieveUpsellOfferDetails(tripBookingId);
            }
            catch (Exception ex)
            {

                var errorResponse = Json.Deserialize<UpsellErrorResponse>(((HttpResponseException)ex.InnerException)?.Response?.Content?.ReadAsStringAsync().GetAwaiter().GetResult());

                //Assert
                Assert.True(errorResponse.Errors.First().Code == "NGTUPSL500");

            }

        }
    }
}