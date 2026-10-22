using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Tests.InfrastructureTests
{
    public class UpdateAPIMSubscriptionRepoCommandTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdateAPIMSubscriptionRepoCommandHandler>> _logger;
        private readonly Mock<APIMSubscriptionConfigHelper> _apimConfiguration;

        public UpdateAPIMSubscriptionRepoCommandTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdateAPIMSubscriptionRepoCommandHandler>>();
            _apimConfiguration = new Mock<APIMSubscriptionConfigHelper>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAPIMSubscriptionRepoCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAPIMSubscriptionRepoCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, JsonConvert.SerializeObject(new APIMSubscriptionResponse { Properties = new SubscriptionProperties { PrimaryKey = "SubscriptionKey", State = "active" } }));
            var updateSubscriptionRequest = new UpdateAPIMSubscriptionRepoCommand(new SubscriptionUpdateCommand { Enabled = "true", SubscriptionId = "SubscriptionId", Channel = "NDC", Environment = "NONPROD" });
            _mediator.Setup(m => m.Send(It.IsAny<UpdateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            UpdateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(updateSubscriptionRequest, CancellationToken.None);

            //Assert
            Assert.Equal("NDC", result.Channel);
            Assert.Equal("NONPROD", result.Environment);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("SubscriptionKey", result.SubscriptionKeys[0].Key);
            Assert.Equal("Enabled", result.SubscriptionState);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNotFoundOccurs()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, "Not Found");
            var updateSubscriptionRequest = new UpdateAPIMSubscriptionRepoCommand(new SubscriptionUpdateCommand { Enabled = "true", SubscriptionId = "SubscriptionId", Channel = "NDC", Environment = "NONPROD" });
            _mediator.Setup(m => m.Send(It.IsAny<UpdateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            UpdateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(updateSubscriptionRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.VE002, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.VE002Reason, result.Error.Message);
            Assert.Equal(Status.OK, result.Status);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenErrorOccurs()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.Created, "Error");
            var updateSubscriptionRequest = new UpdateAPIMSubscriptionRepoCommand(new SubscriptionUpdateCommand { Enabled = "true", SubscriptionId = "SubscriptionId", Channel = "NDC", Environment = "NONPROD" });
            _mediator.Setup(m => m.Send(It.IsAny<UpdateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            UpdateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(updateSubscriptionRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.G039, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.G039Reason, result.Error.Message);
            Assert.Equal(Status.Error, result.Status);
        }
    }
}
