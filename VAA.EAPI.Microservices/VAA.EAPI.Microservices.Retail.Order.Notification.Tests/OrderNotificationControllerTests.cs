using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Notification.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderNotificationController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _request;

        public OrderNotificationControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderNotificationController>>();
            _context = new Mock<FunctionContext>();

            string body = "{\"orderReference\":\"ABCDE\",\"creationDate\":\"2022-04-21\",\"creationTime\":\"2022-04-21T13:59:42.209Z\",\"fulfillmentDetails\":[{\"idType\":\"pnr\",\"fulfillmentId\":\"ABC\"}]}";
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var channelId = new HttpHeadersCollection();

            _request = new Mock<HttpRequestData>(_context.Object);
            _request.Setup(m => m.Body).Returns(bodyStream);
            _request.Setup(m => m.Headers).Returns(channelId);
            _request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationController(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<OrderNotificationRequestCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act
            OrderNotificationController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.OrderNotification(_request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorWhenErrorOccurs()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Error, Errors = new System.Collections.Generic.List<ErrorDetails>() { } };
            _mediator.Setup(m => m.Send(It.IsAny<OrderNotificationRequestCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act
            OrderNotificationController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.OrderNotification(_request.Object);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
