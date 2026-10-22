using Moq;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;


namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class TripsMessageQueryTests
    {
        private readonly Mock<ITripsDetailsQueryProvider> _mockTripsDetailsQueryProvider;
        private readonly TripsMessageQuery _query;

        public TripsMessageQueryTests()
        {
            _mockTripsDetailsQueryProvider = new Mock<ITripsDetailsQueryProvider>();
            _query = new TripsMessageQuery(_mockTripsDetailsQueryProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTripsDetailsQueryProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TripsMessageQuery(null));
        }

        [Fact]
        public async Task GetTripMessageDetails_ShouldThrowNullException_WhenTripReservationIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _query.GetTripMessageDetails(null));
        }

        [Fact]
        public async Task GetTripMessageDetails_ShouldReturnMessageResponse_WithValidInput()
        {
            var tripReservationId = "ee6b78ab-8a03-48d7-9c75-5af9a38c8940";
            var expectedResponse = TestDataProvider.GetTestData<MessageResponse>("Trips\\TripsMessageData");

            _mockTripsDetailsQueryProvider.Setup(x => x.GetTripMessageDetails(tripReservationId)).ReturnsAsync(expectedResponse);

            var result = await _query.GetTripMessageDetails(tripReservationId);

            _mockTripsDetailsQueryProvider.Verify(x => x.GetTripMessageDetails(tripReservationId), Times.Once, $"Expected GetTripMessageDetails to be called once with ID {tripReservationId}.");
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Passengers, result.Passengers);
            Assert.Equal(expectedResponse.Messages, result.Messages);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetTripMessageDetails_ShouldPropagateException_WhenProviderThrows()
        {
            var tripReservationId = "ee6b78ab-8a03-48d7-9c75-5af9a38c8940";
            var expectedException = new Exception("Test exception");

            _mockTripsDetailsQueryProvider.Setup(x =>
                x.GetTripMessageDetails(tripReservationId))
                .ThrowsAsync(expectedException);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _query.GetTripMessageDetails(tripReservationId));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        [Fact]
        public async Task GetFlyReadyStatusAsync_ShouldThrowArgumentNullException_WhenTripReservationIdIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _query.GetFlyReadyStatusAsync(null));
        }

        [Fact]
        public async Task GetFlyReadyStatusAsync_ShouldReturnStatus_WhenInputIsValid()
        {
            // Arrange
            var msTripBookingId = Guid.NewGuid().ToString();
            var tripBookingID = BookingIdHelper
                .CreateBookingId(null, msTripBookingId, "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString())
                .GetAwaiter()
                .GetResult();

            var expectedStatus = new FlyReadyStatus
            {
                GivenName = "givenName",
                Status = "COMPLETED"
            };

            _mockTripsDetailsQueryProvider
                .Setup(x => x.GetFlyreadyTripMessageStatus(It.IsAny<string>())).ReturnsAsync(new List<FlyReadyStatus> { expectedStatus });

            // Act
            var result = await _query.GetFlyReadyStatusAsync(tripBookingID);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedStatus.Status, result.First().Status);
            Assert.Equal(expectedStatus.GivenName, result.First().GivenName);
        }

    }
}
