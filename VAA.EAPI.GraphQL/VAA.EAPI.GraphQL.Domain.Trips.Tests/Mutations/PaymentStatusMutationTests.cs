using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class PaymentStatusMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<PaymentStatusMutation> _loggerPaymentStatusMutation;
        private readonly HttpConnector _httpConnector;
        private readonly IBasketProvider _mockbasketProvider;
        private readonly PaymentStatusMutation _paymentStatusMutation;
       
        public PaymentStatusMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerPaymentStatusMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<PaymentStatusMutation>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockbasketProvider = new BasketProvider(_httpConnector);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _paymentStatusMutation = new PaymentStatusMutation(_mockbasketProvider, _loggerPaymentStatusMutation);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task PaymentStatusMutation_NullRequest_Fails()
        {            
            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _paymentStatusMutation.PaymentStatus(null));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }


        [Fact]
        public async Task PaymentStatusMutation_NoPaidSeatSelected_ThrowsError()
        {
            // Arrange
            string basketId = "228faa37-0729-4edc-bc68-2b24d17d8fb2";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();
         
            var paymentStatusRequest = new PaymentStatusRequest
            {
                TripReservationId = tripReservationId,
                PaymentReferenceId = "SomeReferenceId",
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _paymentStatusMutation.PaymentStatus(paymentStatusRequest));
            Assert.IsType<NotImplementedException>(ex.InnerException);
        }

        [Fact]
        public async Task PaymentStatusMutation_SeatsSelectedNoPRIGenerated_ThrowsError()
        {
            // Arrange
            string basketId = "4d7e9512-f5d2-4572-84e1-b2fcdd773993";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            var paymentStatusRequest = new PaymentStatusRequest
            {
                TripReservationId = tripReservationId,
                PaymentReferenceId = "SomeReferenceId",
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _paymentStatusMutation.PaymentStatus(paymentStatusRequest));
            Assert.IsType<NotImplementedException>(ex.InnerException);
        }

        [Fact]
        public async Task PaymentStatusMutation_PaidSeatsSelectedWithPRIGenerated_ReturnSuccess()
        {
            // Arrange
            string basketId = "0b069078-c303-4d22-8f7d-03e8068c5a55";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            var paymentStatusRequest = new PaymentStatusRequest
            {
                TripReservationId = tripReservationId,
                PaymentReferenceId = "MBDTGOEI6973VPXNCJRHSZW1K45UL0YF",
                PaymentStatus = Models.Enum.PaymentStatus.SUCCESS
            };

            // Act
            var result = await _paymentStatusMutation.PaymentStatus(paymentStatusRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.SUCCESS, result.PaymentStatus);           
        }

        [Fact]
        public async Task PaymentStatusMutation_PaidSeatsSelectedWithPRIGenerated_ReturnsFailure()
        {
            // Arrange
            string basketId = "0b069078-c303-4d22-8f7d-03e8068c5a55";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            var paymentStatusRequest = new PaymentStatusRequest
            {
                TripReservationId = tripReservationId,
                PaymentReferenceId = "MBDTGOEI6973VPXNCJRHSZW1K45UL0YF",
                PaymentStatus = Models.Enum.PaymentStatus.FAILED
            };

            // Act
            var result = await _paymentStatusMutation.PaymentStatus(paymentStatusRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.FAILED, result.PaymentStatus);
        }
    }
}
