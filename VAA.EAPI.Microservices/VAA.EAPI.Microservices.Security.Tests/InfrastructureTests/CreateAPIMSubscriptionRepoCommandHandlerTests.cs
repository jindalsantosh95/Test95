using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Tests.InfrastructureTests
{
    public class CreateAPIMSubscriptionRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CreateAPIMSubscriptionRepoCommandHandler>> _logger;

        public CreateAPIMSubscriptionRepoCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CreateAPIMSubscriptionRepoCommandHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CreateAPIMSubscriptionRepoCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CreateAPIMSubscriptionRepoCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var subscriptionCommandRq = new SubscriptionCreateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", SubscriptionName = "TestAgency-TestAggregator", UserAccount = new UserAccountDetails { FirstName = "Agent1", LastName = "LastName1", EmailAddress = "testagent@agency1.com", UserId = "Agent1" } };
            var apimSubscriptionRS = new APIMSubscriptionResponse { Id = "TestSubscription1", Name = "TestSubscription1Name", Properties = new SubscriptionProperties { CreatedDate = Convert.ToString(DateTime.Now), ExpirationDate = Convert.ToString(DateTime.Now.AddYears(1)), } };
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.Created, JsonConvert.SerializeObject(apimSubscriptionRS));

            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            CreateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(new CreateAPIMSubscriptionRepoCommand(subscriptionCommandRq), CancellationToken.None);

            //Assert
            Assert.Equal("NDC", result.Channel);
            Assert.Equal("NONPROD", result.Environment);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("Enabled", result.SubscriptionState);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNotFoundOccurs()
        {
            //Arrange
            var subscriptionCommandRq = new SubscriptionCreateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", SubscriptionName = "TestAgency-TestAggregator", UserAccount = new UserAccountDetails { FirstName = "Agent1", LastName = "LastName1", EmailAddress = "testagent@agency1.com", UserId = "Agent1" } };
            var apimSubscriptionRS = new APIMSubscriptionResponse { Id = "TestSubscription1", Name = "TestSubscription1Name", Properties = new SubscriptionProperties { CreatedDate = Convert.ToString(DateTime.Now), ExpirationDate = Convert.ToString(DateTime.Now.AddYears(1)), } };
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, "Ok");

            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            CreateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(new CreateAPIMSubscriptionRepoCommand(subscriptionCommandRq), CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.VE001, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.VE001Reason, result.Error.Message);
            Assert.Equal(Status.OK, result.Status);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenErrorOccurs()
        {
            //Arrange
            var subscriptionCommandRq = new SubscriptionCreateCommand { Channel = "NDC", Environment = "NONPROD", SubscriptionId = "Agency123-Aggregator123", SubscriptionName = "TestAgency-TestAggregator", UserAccount = new UserAccountDetails { FirstName = "Agent1", LastName = "LastName1", EmailAddress = "testagent@agency1.com", UserId = "Agent1" } };
            var apimSubscriptionRS = new APIMSubscriptionResponse { Id = "TestSubscription1", Name = "TestSubscription1Name", Properties = new SubscriptionProperties { CreatedDate = Convert.ToString(DateTime.Now), ExpirationDate = Convert.ToString(DateTime.Now.AddYears(1)), } };
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, "Error");

            _mediator.Setup(m => m.Send(It.IsAny<CreateAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            CreateAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(new CreateAPIMSubscriptionRepoCommand(subscriptionCommandRq), CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.G039, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.G039Reason, result.Error.Message);
            Assert.Equal(Status.Error, result.Status);
        }

    }
}
