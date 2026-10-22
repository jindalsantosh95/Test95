using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationStatusQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderNotificationStatusQueryHandler>> _logger;
        private readonly OrderNotificationStatusQueryHandler _handler;

        public OrderNotificationStatusQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderNotificationStatusQueryHandler>>();
            _handler = new OrderNotificationStatusQueryHandler(_mediator.Object, _logger.Object);
            OrderNotificationTestsCommon.CreateTestEnvironmentSettings();
        }


        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationStatusQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationStatusQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).Throws<ArgumentNullException>();
            var result = await _handler.Handle(new OrderNotificationStatusQuery(), CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldReturnCompletedStatusResponse()
        {
            var request = new OrderNotificationStatusQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderNotificationEventSubject
            };
            ConfigReader.Set("MockResponseEnabled", "N");
            var data = new OrderNotificationStatusResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Succeeded"
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderNotificationEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnCompletedStatusResponse_Mock()
        {
            var request = new OrderNotificationStatusQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderNotificationEventSubject
            };
            ConfigReader.Set("MockResponseEnabled", "Y");
            var data = new OrderNotificationStatusResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Succeeded"
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderNotificationEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task Handler_ShouldReturnCompletedStatusResponse_WithoutMock()
        {
            var request = new OrderNotificationStatusQuery
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Type = Constants.Application.OrderNotificationEventSubject
            };
            ConfigReader.Set("MockResponseEnabled", "N");
            var data = new OrderNotificationStatusResponse
            {
                CorrelationId = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c",
                Status = "Succeeded"
            };
            var response = new ServiceResponse { Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.OrderNotificationEventSubject, Id = "7b3f98b8-b487-4ffa-9f89-ab2552f8ca2c", JsonResponseData = Json.Serialize(data) };

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseQuery>(), CancellationToken.None)).ReturnsAsync(response);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
    }
}
