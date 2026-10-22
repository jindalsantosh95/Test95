using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OrderReshopBundleControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderReshopBundleController>> _logger;
        private readonly OrderReshopBundleController _controller;
        private readonly Mock<FunctionContext> _context;


        public OrderReshopBundleControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderReshopBundleController>>();
            _controller = new OrderReshopBundleController(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();
        }
        [Fact]
        public void TestController_Failure()
        {
            //Arrange
            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act
            var result = _controller.OrderReshopBundle(request.Object);

            //Assert
            Assert.False(result.IsCompletedSuccessfully);
        }
        [Fact]
        public async Task TestController_PassedWithOKStatus()
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

            _mediator.Setup(m => m.Send(It.IsAny<OrderReshopBundleQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OfferResponse>() { Status = Status.OK, Value = new OfferResponse() });

            //Act
            var result = await _controller.OrderReshopBundle(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
        [Fact]
        public async Task TestController_WithErrorStatus()
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

            _mediator.Setup(m => m.Send(It.IsAny<OrderReshopBundleQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OfferResponse>() { Status = Status.Error, Errors = new List<ErrorDetails>() });

            //Act
            var result = await _controller.OrderReshopBundle(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
