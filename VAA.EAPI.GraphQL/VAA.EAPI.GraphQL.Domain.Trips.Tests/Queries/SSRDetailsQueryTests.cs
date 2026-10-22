using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class SSRDetailsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ISSRDetailsProvider _mockSSRDetailsQueryProvider;
        private readonly SSRQuery _ssrQuery;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;

        public SSRDetailsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockSSRDetailsQueryProvider = new SSRDetailsProvider(_httpConnector);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _ssrQuery = new SSRQuery(_mockSSRDetailsQueryProvider, _mockReferenceServiceProvider);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task QuerySpecailAssistanceSSRDetailsForTrip_ValidId_ReturnsResponse()
        {
            // Arrange
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "74508453-b3d7-42a7-b058-ca0b3cd88888", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString());
            

            // Act
            var result = await _ssrQuery.GetSpecialAssistance(tripReservationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers.Any());
            Assert.True(result.Passengers.First().PassengerResourceId.Any());
            Assert.True(result.Passengers.First().PassengerSegments.First().ServiceRequests.Any());

            var wheelchairRequest = result.Passengers.First().PassengerSegments.First().ServiceRequests.Where(r => r.CategoryCode == "WCH");
            Assert.NotNull(wheelchairRequest);
            
            var flightSegment = result.Passengers.First().PassengerSegments.First().FlightSegment;
            var flightLeg = flightSegment?.FlightLegs?.FirstOrDefault();
            
            if (flightLeg?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(flightLeg.ScheduledDepartureLocal);
            if (flightLeg?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(flightLeg.ScheduledArrivalLocal);
        }

        [Fact]
        public async Task QuerySpecailMealsSSRDetailsForTrip_ValidId_ReturnsResponse()
        {
            // Arrang            
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "74508453-b3d7-42a7-b058-ca0b3cd88888", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString());

            // Act
            var result = await _ssrQuery.GetSpecialMeals(tripReservationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers.Any());
            Assert.True(result.Passengers.First().PassengerResourceId.Any());
            Assert.NotNull(result.Passengers.First().PassengerSegments.First().MealSelection);
            Assert.Contains("AVML", result.Passengers.First().PassengerSegments.First().MealSelection.SelectedChoice);
        }

        [Fact]
        public async Task QuerySpecialMealsSSRDetailsForTrip_ValidId_ReturnsResponse()
        {
            // Arrang            
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "579479a6-7e3c-49ff-a4bb-b21ec96b6e81", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString());

            // Act
            var result = await _ssrQuery.GetSpecialMeals(tripReservationId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers.Any());
            Assert.True(result.Passengers.First().PassengerResourceId.Any());
            Assert.NotNull(result.Passengers.First().PassengerSegments.First().MealSelection);
            Assert.Contains("KSML", result.Passengers.First().PassengerSegments.First().MealSelection.SelectedChoice);
            Assert.True(result.Passengers.First().PassengerSegments.First().MealSelection.AvailableChoices.Any());
            Assert.Contains("KSML", result.Passengers.First().PassengerSegments.First().MealSelection.AvailableChoices.Select(c => c.Code));
            Assert.Equal("Kosher Meal", result.Passengers.First().PassengerSegments.First().MealSelection.AvailableChoices.First(c => c.Code == "KSML").Description);
        }
    }
}
