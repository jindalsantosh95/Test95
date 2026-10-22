using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Tests.Applications
{
    public class SusbcriptionUpdateEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<SubscriptionUpdateEventCommandHandler>> _logger;

        public SusbcriptionUpdateEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<SubscriptionUpdateEventCommandHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SubscriptionUpdateEventCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SubscriptionUpdateEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var cloudEvent = new SubscriptionUpdateEventCommand { Data = JsonConvert.SerializeObject(new SubscriptionUpdateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", Enabled = "true" }) };
            var apimSubscriptionRS = new SubscriptionResponse { SubscriptionKeys = new List<SubscriptionKey> { new SubscriptionKey { Key = "PrimarySubscriptionKey" } }, Channel = "NDC", Status = Status.OK, Environment = "NONPROD" };

            _mediator.Setup(m => m.Send(It.IsAny<UpdateAPIMSubscriptionRepoCommand>(), CancellationToken.None)).ReturnsAsync(apimSubscriptionRS);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None));

            //Act
            SubscriptionUpdateEventCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(cloudEvent, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Sucess", result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNotFoundOccurs()
        {
            //Arrange
            var cloudEvent = new SubscriptionUpdateEventCommand { Data = JsonConvert.SerializeObject(new SubscriptionUpdateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", Enabled = "true" }) };
            var apimSubscriptionRS = new SubscriptionResponse { Channel = "NDC", Status = Status.BadRequest, Environment = "NONPROD" };

            _mediator.Setup(m => m.Send(It.IsAny<UpdateAPIMSubscriptionRepoCommand>(), CancellationToken.None)).ReturnsAsync(apimSubscriptionRS);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None));

            //Act
            SubscriptionUpdateEventCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(cloudEvent, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
        }
    }
}
