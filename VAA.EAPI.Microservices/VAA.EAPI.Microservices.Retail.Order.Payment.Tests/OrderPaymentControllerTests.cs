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
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderPaymentController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _request;

        public OrderPaymentControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderPaymentController>>();
            _context = new Mock<FunctionContext>();

            string body = "[{\"clientId\":123,\"transactionId\":12,\"transactionDateTime\":\"2022-04-29T15:43:04.974Z\",\"transactionAmount\":15,\"paymentMethod\":\"CreditDebit\",\"paymentType\":\"Card\",\"paymentSubType\":\"Credit\",\"cardName\":\"Mastercard\",\"cardNumber\":\"123456789012\",\"cardExpiry\":\"11-24\",\"status\":\"Authorised\",\"currency\":\"GBP\",\"countryId\":12,\"authorisationCode\":\"987\",\"fraudCode\":34,\"fraudStatus\":\"ABC\",\"pspName\":\"DFE\",\"pspId\":456}]";
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
            new OrderPaymentController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentController(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var orderReference = TestDataProvider.GetTestData<string>("OrderPayment.OrderReferenceId");
            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<OrderPaymentEventCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act
            OrderPaymentController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.OrderPayment(_request.Object, orderReference);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorWhenErrorOccurs()
        {
            //Arrange
            var orderReference = TestDataProvider.GetTestData<string>("OrderPayment.OrderReferenceId");
            var response = new Response<string>() { Status = Status.Error, Errors = new System.Collections.Generic.List<ErrorDetails>() { } };
            _mediator.Setup(m => m.Send(It.IsAny<OrderPaymentEventCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act
            OrderPaymentController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.OrderPayment(_request.Object, orderReference);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
