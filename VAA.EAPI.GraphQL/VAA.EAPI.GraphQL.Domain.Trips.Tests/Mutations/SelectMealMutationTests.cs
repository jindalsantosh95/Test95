using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SelectMealMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ISSRDetailsProvider _mockSSRDetailsQueryProvider;
        private readonly SelectMealMutation _selectMealMutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly ILogger<SelectMealMutation> _mutationLogger;

        public SelectMealMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockSSRDetailsQueryProvider = new SSRDetailsProvider(_httpConnector);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _mutationLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SelectMealMutation>.Instance;
            _selectMealMutation = new SelectMealMutation(_mockSSRDetailsQueryProvider, _mockReferenceServiceProvider, _mutationLogger);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task EditOneEligibleSegmentMealSelectionForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";


            // Act
            var result = await _selectMealMutation.SelectMeal(tripReservationId, segmentId: "1", passengerId: passengerId, ssrCode: "VJML", applyToAll: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").MealSelection.SelectedChoice, "VJML");
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "2").MealSelection.SelectedChoice == "STND");
        }

        [Fact]
        public async Task EditAllEligibleSegmentMealSelectionForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac6", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";

            // Act
            var result = await _selectMealMutation.SelectMeal(tripReservationId, segmentId: "1", passengerId: passengerId, ssrCode: "VJML", applyToAll: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").MealSelection.SelectedChoice, "VJML");
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "2").MealSelection.SelectedChoice == "VJML");
        }


        [Fact]
        public async Task DeleteMealSelectionForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac5", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";


            // Act
            var result = await _selectMealMutation.SelectMeal(tripReservationId, segmentId: "1", passengerId: passengerId, ssrCode: "VJML", applyToAll: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").MealSelection.SelectedChoice, "STND");
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").MealSelection.IsSelected);

            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "2").MealSelection.SelectedChoice == "STND");
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").MealSelection.IsSelected);
        }
    }
}
