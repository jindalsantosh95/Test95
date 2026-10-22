using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Constants;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SeatFulfilmentMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IBasketProvider _basketServiceProvider;
        private readonly ISeatProvider _seatServiceProvider;
        private readonly ILogger<SeatFulfilmentMutation> _logger;
        private readonly ILogger<HttpConnector> _loggerHttpConnector;
        private readonly HttpConnector _httpConnector;
        private readonly IPaymentServiceProvider _mockpaymentServiceProvider;
        private readonly SeatFulfilmentMutation _seatFulfilmentMutation;
        private readonly IReferenceServiceProvider _mockreferenceServiceProvider;
        private readonly ISeatUpgradeHandler _seatUpgradeHandler;

        public SeatFulfilmentMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _loggerHttpConnector = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_loggerHttpConnector, httpClientFactoryFixture.Current);
            _mockpaymentServiceProvider = new PaymentServiceProvider(_httpConnector);
            _basketServiceProvider = new BasketProvider(_httpConnector);
            _seatServiceProvider = new SeatProvider(_httpConnector);
            _mockreferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SeatFulfilmentMutation>.Instance;
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _seatUpgradeHandler = new SeatUpgradeHandler();
            _seatFulfilmentMutation = new SeatFulfilmentMutation(_basketServiceProvider, _seatServiceProvider, _logger, _mockpaymentServiceProvider, _mockreferenceServiceProvider, _seatUpgradeHandler);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task SeatFulfilmentMutation_NullRequest_ReturnFailure()
        {             
            // Act & Assert            
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(null));

        }

        [Fact]
        public async Task SeatFulfilmentMutation_NullTripReservationId_ReturnFailure()
        {
            // Arrange
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = "123"
            };

            // Act & Assert            
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest));

        }

        [Fact]
        public async Task SeatFulfilmentPaid_WithoutPaymentStatus_ReturnFailure()
        {
            // Arrange
            string basketId = "9dc99361-dd15-49f1-b24b-cf5859ba49ce";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(null));
        }

        [Fact]
        public async Task SeatFulfilment_WithFailurePaymentStatus_ReturnFailure()
        {
            // Arrange
            string basketId = "9dc99361-dd15-49f1-b24b-cf5859ba49ce";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(null));
        }

        [Fact]
        public async Task SeatFulfilmentPaid_ReturnSuccess()
        {
            // Arrange
            string basketId = "9dc99361-dd15-49f1-b24b-cf5859ba49ce";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest); ;

            // Assert            
            Assert.True(result.Errors == null);
        }

        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_HasFlightSegment()
        {
            // Arrange
            string basketId = "9dc99361-dd15-49f1-b24b-cf5859ba49ce";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.FlightSegments.Any());
        }

        [Fact]
        public async Task SeatFulfilmentFree_HasFlightSegmentWithConfirmedSeats()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
        }

        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_HasFareProductNameWithConfirmedSeats()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
            Assert.True(result.PassengerSeatSelection.Where(x => x.IsConfirmed == true).All(a => !string.IsNullOrEmpty(a.SelectedFareProductName)));
        }

        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_ConfirmedSeatsFlightSegmentPresent()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
            Assert.True(result.FlightSegments.Any(a => result.PassengerSeatSelection.Where(a => a.IsConfirmed.Value).Select(s => s.SegmentId).Contains(a.Id)));
        }

        [Fact]
        public async Task SeatFulfilmentFree_ConfirmedSeats_PartialSuccess()
        {
            // Arrange
            string basketId = "6775a735-c8bd-4900-9a64-422a7ca8f824";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "62175fcc-b779-4121-9954-e77c39e5a759", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);
           

            // Assert            
            Assert.NotNull(result);
            Assert.True(result.PassengerSeatSelection.Any());
            Assert.Contains(result.Warnings, w => w.Type == SeatResponseWarnings.PartialSeatConfirmation.ToString());
        }

        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_ConfirmedSeats_NotUpgradeReturnSuccess()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "FAILED", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
            Assert.True(result.FlightSegments.Any(a => result.PassengerSeatSelection.Where(a => a.IsConfirmed.Value).Select(s => s.SegmentId).Contains(a.Id)));
        }

        [Fact]
        public async Task SeatFulfilmentUpgradeSeatFulfilment_NotAllSeatsUpgraded_ReturnFailure()
        {
            // Arrange
            string basketId = "059d790f-23a3-4b6d-899f-e0e7610f3ea4";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };
           
            // Act & Assert            
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest));
           
        }

        [Fact]
        public async Task SeatFulfilmentSeatFulfilment_ConfirmSeats_AllFailure()
        {
            // Arrange
            string basketId = "c07eefb5-dfc3-45c5-8cfc-19c8dae25830";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "DLI9WL", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act & Assert            
            await Assert.ThrowsAsync<Exception>(() => _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest));

        }

        [Fact]
        public async Task SeatFulfilmentSeatFulfilment_ConfirmSeatsIncomleteSeatSuccess_ReturnsSuccessWithWarning()
        {
            // Arrange
            string basketId = "7759ccf6-3e45-40be-901b-e76b009ef230";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "CJBR8A", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert
            Assert.True(result.Warnings != null);
            Assert.Contains(result.Warnings, w => w.Type == SeatResponseWarnings.SeatConfirmationSuccessWithWarnings.ToString());
        }

        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_SuccessWithAirlineDataFromReference()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
            Assert.True(result.FlightSegments.Any(a => result.PassengerSeatSelection.Where(a => a.IsConfirmed.Value).Select(s => s.SegmentId).Contains(a.Id)));
            Assert.Equal("Virgin Atlantic For Test", result.FlightSegments?.FirstOrDefault()?.OperatingCarrier?.CarrierName);

        }


        [Fact]
        public async Task SeatFulfilmentFreeSeatFulfilment_Case5_ConfirmedSeatsFlightSegmentPresent()
        {
            // Arrange
            string basketId = "5e056f19-30f0-44f4-9cbd-cbc4418f8b21";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac9", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
            var seatFulfilmentRequest = new SeatConfirmationRequest()
            {
                TripReservationId = tripReservationId,
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _seatFulfilmentMutation.ConfirmSeats(seatFulfilmentRequest);

            // Assert            
            Assert.True(result.Errors == null);
            Assert.True(result.PassengerSeatSelection.Any(a => a.IsConfirmed.Value));
            Assert.True(result.FlightSegments.Any(a => result.PassengerSeatSelection.Where(a => a.IsConfirmed.Value).Select(s => s.SegmentId).Contains(a.Id)));
        }
    }
}

