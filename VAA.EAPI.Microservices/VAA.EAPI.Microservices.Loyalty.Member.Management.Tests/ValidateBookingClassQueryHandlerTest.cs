using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class ValidateBookingClassQueryHandlerTest
    {
        private readonly Mock<ILogger<ValidateBookingClassQueryHandler>> _mockLogger;
        private readonly Mock<ICommonRefDBServices> _mockCommonRefDBServices;
        private readonly ValidateBookingClassQueryHandler _handler;

        public ValidateBookingClassQueryHandlerTest()
        {
            _mockLogger = new Mock<ILogger<ValidateBookingClassQueryHandler>>();
            _mockCommonRefDBServices = new Mock<ICommonRefDBServices>();
            _handler = new ValidateBookingClassQueryHandler(_mockCommonRefDBServices.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateBookingClassQueryHandler(_mockCommonRefDBServices.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCommonRefDBServicesObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateBookingClassQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var bookingDetailsResult = new BookingDetailsResult
            {
                CabinClass = "Economy",
                BookClass = "BOOKINGCLASS",
                OriginCountryCode = "US",
                DestinationCountryCode = "UK",
                DistanceInMiles = 3000
            };

            _mockCommonRefDBServices.Setup(m => m.ProcessBookingClassData(
                It.IsAny<PreValidationPartnerData>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(bookingDetailsResult);

            ValidateBookingClassQuery query = new()
            {
                ValidationRequest = new BookingClassValidationRequest
                {
                    PartnerBookingData = new PreValidationPartnerData(),
                    bookingClass = "Y",
                    flownClass = "Y",
                    activityDate = DateTime.Now,
                    listFareValue = "FARE",
                    listTicketValue = "TICKET",
                    listPaxValue = "PAX"
                }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(bookingDetailsResult, result.Value.Value);
        }

        [Fact]
        public async Task HandlerFailureWithException()
        {
            _mockCommonRefDBServices.Setup(m => m.ProcessBookingClassData(
                It.IsAny<PreValidationPartnerData>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            ValidateBookingClassQuery query = new()
            {
                ValidationRequest = new BookingClassValidationRequest
                {
                    PartnerBookingData = new PreValidationPartnerData(),
                    bookingClass = "Y",
                    flownClass = "Y",
                    activityDate = DateTime.Now,
                    listFareValue = "FARE",
                    listTicketValue = "TICKET",
                    listPaxValue = "PAX"
                }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
            Assert.False(result.Value.IgnoreNullValues);
        }
    }
}

