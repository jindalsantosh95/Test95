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
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests
{
    public class CustomerAssignAwardControllerTests
    {

        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CustomerAssignAwardController>> _logger;
        private readonly Mock<CustomerAssignAwardController> _handler;

        public CustomerAssignAwardControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CustomerAssignAwardController>>();
            _handler = new Mock<CustomerAssignAwardController>(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardController(_mediator.Object, null));
        }

        [Fact]
        public async Task ShouldReturnAcceptedForGoodRequest()
        {
            //Arrange

            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<CustomerAssignAwardCommand>(), CancellationToken.None)).ReturnsAsync(response);

            string body = "{\"type\":\"abc\",\"source\":\"xyz\",\"subject\":\"xyz\",\"id\":\"051675-ab15-4ef1-be13-854abf56254d\",\"time\":\"2022-02-09T14:26:13.4770788Z\",\"datacontenttype\":\"application/json\",\"data\":{\"memberId\":\"123\",\"productName\":\"Top Member\"}}";
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);

            Mock<FunctionContext> context = new();
            var req = new Mock<HttpRequestData>(context.Object);
            req.Setup(m => m.Body).Returns(bodyStream);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            //Act

            CustomerAssignAwardController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.CustomerAssignAward(req.Object);

            //Assert

            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }
    }
}
