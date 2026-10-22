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
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class CustomerBookingControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CustomerBookingController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly CustomerBookingController _controller;
        private readonly Mock<HttpRequestData> _request;

        public CustomerBookingControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CustomerBookingController>>();
            _context = new Mock<FunctionContext>();
            _controller = new CustomerBookingController(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerBookingController(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerBookingController(_mediator.Object, null));
        }
        [Fact]
        public void Controller_ShouldFail_WithBadRequest()
        {
            //Arrange

            var request = new Mock<HttpRequestData>(_context.Object); //Empty request
            CustomerBookingController controller = new CustomerBookingController(_mediator.Object, _logger.Object);

            //Act

            var result = controller.GetBooking(request.Object);

            //Assert

            Assert.False(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task TestController_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var response1 = TestDataProvider.GetTestData<LastNextBookingQuery>("OrderSearchMS\\GetBookingQueryData");

            string body = TestDataProvider.GetTestData<string>("OrderSearchMS\\CustomerBookingControllerData");
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);
            var request = new Mock<HttpRequestData>(_context.Object);
            List<KeyValuePair<string, string>> headers = new();
            {
                headers.Add(new KeyValuePair<string, string>("fCNumber", "dummy"));
                headers.Add(new KeyValuePair<string, string>("phoneNo", "dummy"));

            };
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(headers));
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(_context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            _mediator.Setup(m => m.Send(It.IsAny<OrderDoc>(), System.Threading.CancellationToken.None)).ReturnsAsync(response1);

            //Act
            var result = await _controller.GetBooking(request.Object);
            //Assert
            Assert.NotNull(result);

        }
    }
}
