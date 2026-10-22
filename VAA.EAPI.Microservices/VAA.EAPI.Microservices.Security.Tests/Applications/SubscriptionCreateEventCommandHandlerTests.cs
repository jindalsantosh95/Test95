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
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMUser;

namespace VAA.EAPI.Microservices.Security.Tests.Applications
{
    public class SubscriptionCreateEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<SubscriptionCreateEventCommandHandler>> _logger;

        public SubscriptionCreateEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<SubscriptionCreateEventCommandHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SubscriptionCreateEventCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SubscriptionCreateEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange

            var cloudEvent = new SubscriptionCreateEventCommand { Data = JsonConvert.SerializeObject(new SubscriptionCreateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", SubscriptionName = "TestAgency-TestAggregator", UserAccount = new UserAccountDetails { FirstName = "Agent1", LastName = "LastName1", EmailAddress = "testagent@agency1.com", UserId = "Agent1" } }) };
            var apimSubscriptionRS = new SubscriptionResponse { SubscriptionKeys = new List<SubscriptionKey> { new SubscriptionKey { Key = "PrimarySubscriptionKey" } }, Channel = "NDC", Status = Status.OK, Environment = "NONPROD" };

            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMUserRepoCommand>(), CancellationToken.None)).ReturnsAsync("APIMUserId");
            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMSubscriptionRepoCommand>(), CancellationToken.None)).ReturnsAsync(apimSubscriptionRS);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None));
            //Act
            SubscriptionCreateEventCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(cloudEvent, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Sucess", result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNotFoundOccurs()
        {
            //Arrange
            var cloudEvent = new SubscriptionCreateEventCommand { Data = JsonConvert.SerializeObject(new SubscriptionCreateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", SubscriptionName = "TestAgency-TestAggregator", UserAccount = new UserAccountDetails { FirstName = "Agent1", LastName = "LastName1", EmailAddress = "testagent@agency1.com", UserId = "Agent1" } }) };
            var apimSubscriptionRS = new SubscriptionResponse { Channel = "NDC", Status = Status.BadRequest, Environment = "NONPROD" };

            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMUserRepoCommand>(), CancellationToken.None)).ReturnsAsync("APIMUserId");
            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMSubscriptionRepoCommand>(), CancellationToken.None)).ReturnsAsync(apimSubscriptionRS);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None));

            //Act
            SubscriptionCreateEventCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(cloudEvent, CancellationToken.None);

            //Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
        }
    }
}
