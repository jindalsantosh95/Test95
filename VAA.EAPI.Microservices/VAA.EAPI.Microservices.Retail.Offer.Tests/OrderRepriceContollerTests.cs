using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OrderRepriceContollerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderRepriceController>> _logger;
        private readonly OrderRepriceController _controller;
        private readonly Mock<FunctionContext> _context;

        public OrderRepriceContollerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderRepriceController>>();
            _controller = new OrderRepriceController(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderRepriceController(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new OrderRepriceController(_mediator.Object, null));
        }
        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange

            var request = new Mock<HttpRequestData>(_context.Object); //Empty request
            OrderRepriceController controller = new OrderRepriceController(_mediator.Object, _logger.Object);

            //Act

            var result = controller.OrderReprice(request.Object);

            //Assert

            Assert.False(result.IsCompletedSuccessfully);
        }
        [Fact]
        public async Task OrderRepriceController_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            string body = TestDataProvider.GetTestData<string>("OfferMS\\OrderRepriceControllerRequestData");

            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            _mediator.Setup(m => m.Send(It.IsAny<OrderRepriceQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OfferResponse>() { Status = Status.OK, Value = new OfferResponse() });

            //Act
            var result = await _controller.OrderReprice(request.Object);
            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
        [Fact]
        public async Task OrderRepriceController_ShouldFail_WithBadRequest()
        {
            //Arrange
            string body = "{\"dummy\":\"dummy\"}";
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            _mediator.Setup(m => m.Send(It.IsAny<OrderRepriceQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OfferResponse>() { Status = Status.Error, Errors = new List<ErrorDetails>() });

            //Act
            var result = await _controller.OrderReprice(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }

    }
}
