using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentStatusResponseQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderPaymentStatusResponseQueryHandler>> _logger;
        private readonly OrderPaymentStatusResponseQueryHandler _handler;

        public OrderPaymentStatusResponseQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderPaymentStatusResponseQueryHandler>>();
            _handler = new OrderPaymentStatusResponseQueryHandler(_mediator.Object, _logger.Object);
            OrderPaymentTestsCommon.CreateTestEnvironmentSettings();
        }


        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentStatusResponseQueryHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentStatusResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).Throws<ArgumentNullException>();


            var result = await _handler.Handle(new OrderPaymentStatusResponseQuery(), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldReturnCompletedStatusResponse()
        {
            var request = new OrderPaymentStatusResponseQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderPaymentUpdateEventSubject
            };

            var data = new OrderPaymentResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Succeeded",
                Data = null
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderPaymentUpdateEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);


            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedStatusResponse()
        {
            var request = new OrderPaymentStatusResponseQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderPaymentUpdateEventSubject
            };

            var data = new OrderPaymentResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Succeeded",
                Data = null
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.UpdatePending, Type = Constants.Application.OrderPaymentUpdateEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);


            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorStatusResponse()
        {
            var request = new OrderPaymentStatusResponseQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderPaymentUpdateEventSubject
            };

            var data = new OrderPaymentResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Failed",
                Data = null
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.OrderPaymentUpdateEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);


            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
        }
    }
}
