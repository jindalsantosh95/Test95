using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Triggers.Http;

namespace VAA.EAPI.Microservices.Security.Tests.ControllerTests
{
    public class APIMCreateSubscriptionControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<APIMSubscriptionController>> _logger;
        private readonly Mock<FunctionContext> _context;
        private readonly Mock<HttpRequestData> _request;

        public APIMCreateSubscriptionControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<APIMSubscriptionController>>();
            _context = new Mock<FunctionContext>();

            string body = "{\n    \"SubscriptionId\": \"12345\",\n    \"SubscriptionName\": \"TestAgency\",\n    \"UserAccount\": {\n        \"FirstName\": \"NDCPOCTest\",\n        \"LastName\": \"User\",\n        \"EmailAddress\": \"testuser12@gmail.com\"\n    },\n    \"Channel\": \"NDC\",\n    \"Environment\": \"NONPROD\"\n}";
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
            new APIMSubscriptionController(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new APIMSubscriptionController(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Accepted };
            _mediator.Setup(m => m.Send(It.IsAny<SubscriptionCreateCommand>(), CancellationToken.None)).ReturnsAsync(response);
            var channelId = "NDC";
            var environment = "NONPROD";

            //Act
            APIMSubscriptionController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.CreateSubscription(_request.Object, channelId, environment);

            //Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorWhenErrorOccurs()
        {
            //Arrange
            var response = new Response<string>() { Status = Status.Error, Errors = new List<ErrorDetails>() { } };
            _mediator.Setup(m => m.Send(It.IsAny<SubscriptionCreateCommand>(), CancellationToken.None)).ReturnsAsync(response);
            var channelId = "NDC";
            var environment = "NONPROD";

            //Act
            APIMSubscriptionController controller = new(_mediator.Object, _logger.Object);
            var result = await controller.CreateSubscription(_request.Object, channelId, environment);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }
    }
}
