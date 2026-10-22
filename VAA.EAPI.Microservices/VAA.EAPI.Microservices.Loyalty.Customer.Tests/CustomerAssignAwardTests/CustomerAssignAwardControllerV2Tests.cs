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
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class CustomerAssignAwardControllerV2Tests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CustomerAssignAwardControllerV2>> _logger;
        private readonly Mock<CustomerAssignAwardControllerV2> _handler;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _request;

        public CustomerAssignAwardControllerV2Tests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CustomerAssignAwardControllerV2>>();
            _handler = new Mock<CustomerAssignAwardControllerV2>(_mediator.Object, _logger.Object);
            _context = new Mock<FunctionContext>();

            string body = "{\"memberId\":\"123\",\"productName\":\"Top Member\"}";
            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);

            _request = new Mock<HttpRequestData>(_context.Object);
            _request.Setup(m => m.Body).Returns(bodyStream);
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
            new CustomerAssignAwardControllerV2(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardController(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange

            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<LoyaltyCustomerAssignAwardCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act

            CustomerAssignAwardControllerV2 controller = new(_mediator.Object, _logger.Object);
            var result = await controller.LoyaltyCustomerAssignAward(_request.Object);

            //Assert

            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task Handler_ShouldReturnErrorWhenErrorOccurs()
        {
            //Arrange

            var response = new Response<string>() { Status = Status.Error, Errors = new System.Collections.Generic.List<ErrorDetails>() { } };
            _mediator.Setup(m => m.Send(It.IsAny<LoyaltyCustomerAssignAwardCommand>(), CancellationToken.None)).ReturnsAsync(response);

            //Act

            CustomerAssignAwardControllerV2 controller = new(_mediator.Object, _logger.Object);
            var result = await controller.LoyaltyCustomerAssignAward(_request.Object);

            //Assert

            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);

        }
    }
}
