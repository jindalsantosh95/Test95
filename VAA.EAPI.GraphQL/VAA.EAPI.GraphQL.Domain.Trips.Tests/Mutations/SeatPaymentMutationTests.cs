using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Moq;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SeatPaymentMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<SeatPaymentMutation> _loggerSeatPaymentMutation;
        private readonly HttpConnector _httpConnector;
        private readonly IBasketProvider _mockbasketProvider;
        private readonly IPaymentServiceProvider _mockpaymentServiceProvider;
        private readonly SeatPaymentMutation _paymentMutation;
        private readonly ISeatProvider _mockseatProvider;
        private readonly IReferenceServiceProvider _mockreferenceProvider;

        public SeatPaymentMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerSeatPaymentMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<SeatPaymentMutation>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockbasketProvider = new BasketProvider(_httpConnector);
            _mockpaymentServiceProvider = new PaymentServiceProvider(_httpConnector);
            _mockseatProvider = new SeatProvider(_httpConnector);
            _mockreferenceProvider = new ReferenceServiceProvider(_httpConnector);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _paymentMutation = new SeatPaymentMutation(_mockbasketProvider, _mockpaymentServiceProvider, _loggerSeatPaymentMutation, _mockseatProvider, _mockreferenceProvider);
            Setup.CreateTestEnvironmentSettings();
        }
          
        [Fact]
        public async Task SeatPaymentMutation_NoSeatsSelected_ReturnsNoHPPLink()
        {
            // Arrange
            string basketId = "da0509b2-9e9c-4cd5-a3b3-1410e6810875";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {              
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };

            // Act
            var result = await _paymentMutation.SeatPayment(paymentRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.Amount.MonetaryAmount);
            Assert.Null(result.PaymentReferenceId);
            Assert.Null(result.HppLink);
        }

        [Fact]
        public async Task SeatPaymentMutation_SeatsSelected_ReturnsHPPLink()
        {
            // Arrange
            string basketId = "4d7e9512-f5d2-4572-84e1-b2fcdd773993";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();        

            PaymentRequest paymentRequest = new()
            {              
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };

            // Act
            var result = await _paymentMutation.SeatPayment(paymentRequest);           

            // Assert           
            Assert.NotNull(result);
            Assert.Equal(24.0m, result.Amount.MonetaryAmount);
            Assert.Equal("MBDTGOEI6973VPXNCJRHSZW1K45UL0YF", result.PaymentReferenceId);
            Assert.NotNull(result.HppLink);
            Assert.NotNull(result.PaymentURL);
        }

        [Fact]
        public async Task SeatPaymentMutation_FreeSeatsSelected_NotReturnsHPPLink()
        {
            // Arrange
            string basketId = "7ad3fd69-0724-4c74-999c-39b6332b321d";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {               
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };

            // Act
            var result = await _paymentMutation.SeatPayment(paymentRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.Amount.MonetaryAmount);
            Assert.Null(result.PaymentReferenceId);
            Assert.Null(result.HppLink);
            Assert.Null(result.PaymentURL);
        }

        [Fact]
        public async Task SeatPaymentMutation_PaidAndFreeSeatsSelected_ReturnsHPPLink()
        {
            // Arrange
            string basketId = "ccf9ddc1-9f2f-4107-af7f-afb1e7218036";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };

            // Act
            var result = await _paymentMutation.SeatPayment(paymentRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(29.0m, result.Amount.MonetaryAmount);
            Assert.Equal("MBDTGOEI6973VPXNCJRHSZW1K45UL0YF", result.PaymentReferenceId);
            Assert.NotNull(result.HppLink);
            Assert.NotNull(result.PaymentURL);
        }

        [Fact]
        public async Task SeatPaymentMutation_FailsIfCurrencyCodeNotAvailable()
        {
            // Arrange
            string basketId = "cb50ded5-34cf-407a-8801-0f9a0d1babea";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {                
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _paymentMutation.SeatPayment(paymentRequest));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task SeatPaymentMutation_FailsIfCurrencyNumberNotAvailable()
        {
            // Arrange
            string basketId = "4c836dda-9ef0-4969-a1f6-45fc9dbf7789";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {
                PaymentFailureLink = "https://www.virginatlantic.com/gb/en.html",
                PaymentSuccessLink = "https://www.virginatlantic.com/gb/en.html",
                TripReservationId = tripReservationId
            };
                      

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _paymentMutation.SeatPayment(paymentRequest));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task SeatPaymentMutation_DLFreeSeatsSelected_NotReturnsHPPLink()
        {
            // Arrange
            string basketId = "eba8146c-de78-4db8-ab32-15c386932fbc";
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId).GetAwaiter().GetResult();

            PaymentRequest paymentRequest = new()
            {
                PaymentFailureLink = "",
                PaymentSuccessLink = "",
                TripReservationId = tripReservationId
            };

            // Act
            var result = await _paymentMutation.SeatPayment(paymentRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.Amount.MonetaryAmount);
            Assert.Null(result.PaymentReferenceId);
            Assert.Null(result.HppLink);
            Assert.Null(result.PaymentURL);
        }

        [Fact]
        public async Task SeatPaymentMutation_EmailAddress_UpdatedInBasket()
        {
            // Arrange
            string basketId = "ccf9ddc1-9f2f-4107-af7f-afb1e7218036";
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, basketId);           
            string basketEmailAfterPayment = "updated.email@example.com";

            var realBasketProvider = new BasketProvider(_httpConnector);
            var basketProviderMock = new Mock<IBasketProvider>(MockBehavior.Strict);
            basketProviderMock
                .Setup(x => x.UpdateContactAsync(It.IsAny<ContactUpdateRequest>()))
                .Returns<ContactUpdateRequest>(req => realBasketProvider.UpdateContactAsync(req));
            basketProviderMock
                .Setup(x => x.RetrieveBasketAsync(It.IsAny<string>()))
                .Returns<string>(id => realBasketProvider.RetrieveBasketAsync(id));
            basketProviderMock
                .Setup(x => x.UpdatePaymentDetailsAsync(It.IsAny<UpdateBasketPaymentRequest>()))
                .Returns<UpdateBasketPaymentRequest>(req => realBasketProvider.UpdatePaymentDetailsAsync(req));

            var paymentMutation = new SeatPaymentMutation(
                basketProviderMock.Object,
                _mockpaymentServiceProvider,
                _loggerSeatPaymentMutation,
                _mockseatProvider,
                _mockreferenceProvider);

            PaymentRequest paymentRequest = new()
            {
                PaymentFailureLink = "",
                PaymentSuccessLink = "",
                TripReservationId = tripReservationId,
                EmailAddress = basketEmailAfterPayment
            };

            // Act
            var result = await paymentMutation.SeatPayment(paymentRequest);

            // Assert
            Assert.NotNull(result);
            
            Assert.Equal(29.0m, result.Amount.MonetaryAmount);
            Assert.Equal("MBDTGOEI6973VPXNCJRHSZW1K45UL0YF", result.PaymentReferenceId);
            Assert.NotNull(result.HppLink);
            Assert.NotNull(result.PaymentURL);

            basketProviderMock.Verify(x => x.UpdateContactAsync(It.Is<ContactUpdateRequest>(r =>
                r.BasketId == basketId &&
                r.ContactDetails.EmailAddress == basketEmailAfterPayment)), Times.Once);
            basketProviderMock.Verify(x => x.RetrieveBasketAsync(It.IsAny<string>()), Times.Never);
        }

    }
}
