using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SelectSeatMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<SelectSeatMutation> _loggerSelectSeatMutation;
        private readonly HttpConnector _httpConnector;
        private readonly IBasketProvider _mockbasketProvider;
        private readonly SelectSeatMutation _selectSeatMutation;
        private readonly ISeatProvider _mockseatProvider;
        private readonly IReferenceServiceProvider _mockreferenceProvider;
        private readonly ILOPAProvider _mocklopaServiceProvider;
        private readonly ISeatUpgradeHandler _seatUpgradeHandler;

        public SelectSeatMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerSelectSeatMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<SelectSeatMutation>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockbasketProvider = new BasketProvider(_httpConnector);
            _mockseatProvider = new SeatProvider(_httpConnector);
            _mockreferenceProvider = new ReferenceServiceProvider(_httpConnector);
            _mocklopaServiceProvider = new LOPAProvider(_httpConnector);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _seatUpgradeHandler = new SeatUpgradeHandler();
            _selectSeatMutation = new SelectSeatMutation(_mockbasketProvider, _mockseatProvider, _mocklopaServiceProvider, _mockreferenceProvider, _loggerSelectSeatMutation, _seatUpgradeHandler);
            Setup.CreateTestEnvironmentSettings();
        }
          
        [Fact]
        public async Task SelectSeatMutation_NullRequest_ThrowsException()
        {
            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _selectSeatMutation.SelectSeat(null));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task SelectSeatMutation_SeatsSelected_ReturnsSeatNumber()
        {
            // Arrange
            string basketId = "4d7e9512-f5d2-4572-84e1-b2fcdd773993";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            var paxId = "c7f4f01b-6737-4221-a814-35935648344e";
            SeatSelectionRequest seatSelectRequest = new SeatSelectionRequest()
            {
                TripReservationId = tripReservationId,
                PassengerId = paxId,
                SeatOfferId = "0668ce5a-c3ad-47a6-b0ed-a3371654733e",
                SegmentId = "1",
                SeatNumber = "46D"
            };

            // Act
            var result = await _selectSeatMutation.SelectSeat(seatSelectRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("46D", result.FlightPassengerSeatMap.First(p => p.PassengerId == paxId).SeatNumber);
            Assert.Equal("A", result.FlightPassengerSeatMap.First(p => p.PassengerId == paxId).Pos);
            Assert.NotNull(result.FlightPassengerSeatMap.First(p => p.PassengerId == paxId).Price);
            Assert.False(result.IsSeatCabinChangeRequested);
        }

        [Fact]
        public async Task SelectSeatMutation_SeatSelectionCabinUpgrade_MultiPax_CabinChangeFalseRequest()
        {
            // Arrange
            string basketId = "9b9c577f-463b-42db-8ea7-3f0edc360c12";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            var paxId = "92979390-0ebc-4789-b7f5-b8984047c770";
            SeatSelectionRequest seatSelectRequest = new SeatSelectionRequest()
            {
                TripReservationId = tripReservationId,
                PassengerId = paxId,
                SeatOfferId = "bf103ef9-e3fc-4e7d-bc13-91434c9a974e",
                SegmentId = "1",
                SeatNumber = "54C"
            };

            // Act
            var result = await _selectSeatMutation.SelectSeat(seatSelectRequest);

            // Assert
            Assert.NotNull(result);           
            Assert.False(result.IsSeatCabinChangeRequested);
        }
    }
}
