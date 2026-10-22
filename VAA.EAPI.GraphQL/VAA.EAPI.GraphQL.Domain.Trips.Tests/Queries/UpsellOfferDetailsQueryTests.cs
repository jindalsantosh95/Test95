using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class UpsellOfferDetailsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IUpsellOfferDetailsProvider _mcokUpsellOfferDetailsProvider;
        private readonly UpsellOffersQuery _upsellOffersQuery;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly ILogger<UpsellOffersQuery> _loggerUpsell;

        public UpsellOfferDetailsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerUpsell = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpsellOffersQuery>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mcokUpsellOfferDetailsProvider = new UpsellOfferDetailsProvider(_httpConnector);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _upsellOffersQuery = new UpsellOffersQuery(_mcokUpsellOfferDetailsProvider, _mockReferenceServiceProvider, _loggerUpsell);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task QuerySpecailAssistanceSSRDetailsForTrip_ValidId_ReturnsResponse()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "9092c982-2b7f-417c-a6a6-ad0554c53471", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();


            // Act
            var result = await _upsellOffersQuery.GetUpsellOfferDetails(tripReservationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UpsellEligible == true && result.Segments.FirstOrDefault(s => s.OperatingCarrier.CarrierCode == "VS").UpsellOffers.Count() > 0);
            
            var upsellSegment = result.Segments?.FirstOrDefault();
            var upsellLeg = upsellSegment?.Legs?.FirstOrDefault();
            var upsellSlice = result.Slices?.FirstOrDefault();
            
            if (upsellSlice?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(upsellSlice.ScheduledDepartureLocal);
            if (upsellSlice?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(upsellSlice.ScheduledArrivalLocal);
            if (upsellLeg?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(upsellLeg.ScheduledDepartureLocal);
            if (upsellLeg?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(upsellLeg.ScheduledArrivalLocal);
        }
    }
}
