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
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class OrderSearchControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderSearchController _controller;
        private readonly Mock<FunctionContext> _context;


        public OrderSearchControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderSearchController(_mediator.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void TestController_Failure()
        {
            //Arrange

            var request = new Mock<HttpRequestData>(_context.Object); //Empty request

            //Act

            var result = _controller.OrderSearch(request.Object);

            //Assert

            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task TestController_PassedWithOKStatus()
        {
            //Arrange

            string body = "{\"SearchFilterCriteria\":{\"bookingReferenceCriteria\":{\"BookingReference\":{\"AirlineDesignator\":null,\"RLOC\":\"dummy\",\"RLOCCreateDate\":\"2022-05-05T10:01:37.8548871Z\",\"ExternalRecordLocator\":null},\"customerName\":{\"givenName\":\"Test\",\"surName\":\"Mock\"}},\"orderReferenceCriteria\":{\"orderID\":\"dummy\",\"ownerCode\":\"dummy\"}}}";
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

            _mediator.Setup(m => m.Send(It.IsAny<OrderSearchResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OrderSearchResponse>() { Status = Status.OK, Value = new OrderSearchResponse() });

            //Act
            var result = await _controller.OrderSearch(request.Object);
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

            _mediator.Setup(m => m.Send(It.IsAny<OrderSearchResponseQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OrderSearchResponse>() { Status = Status.Error, Errors = new List<ErrorDetails>() });

            //Act
            var result = await _controller.OrderSearch(request.Object);
            //Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, result.StatusCode);

        }
    }
}
