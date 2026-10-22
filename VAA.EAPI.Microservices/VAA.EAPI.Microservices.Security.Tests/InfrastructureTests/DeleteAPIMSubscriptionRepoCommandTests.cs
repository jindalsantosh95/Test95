using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Command.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Tests.InfrastructureTests
{
    public class DeleteAPIMSubscriptionRepoCommandTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<DeleteAPIMSubscriptionRepoCommandHandler>> _logger;
        private readonly Mock<FunctionContext> _context;

        public DeleteAPIMSubscriptionRepoCommandTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<DeleteAPIMSubscriptionRepoCommandHandler>>();
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new DeleteAPIMSubscriptionRepoCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new DeleteAPIMSubscriptionRepoCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenErrorsOccur()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, "Deleted");
            var deleteSubscriptionReq = new DeleteAPIMSubscriptionRepoCommand("URl", new SubscriptionDeleteCommand { SubscriptionId = "Subscription1" });
            _mediator.Setup(m => m.Send(It.IsAny<DeleteAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            DeleteAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(deleteSubscriptionReq, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.G039, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.G039Reason, result.Error.Message);
            Assert.Equal(Status.Error, result.Status);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, "Deleted");
            var deleteSubscriptionReq = new DeleteAPIMSubscriptionRepoCommand("URl", new SubscriptionDeleteCommand { SubscriptionId = "Subscription1", Channel = "NDC", Environment = "NONPROD" });
            _mediator.Setup(m => m.Send(It.IsAny<DeleteAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            DeleteAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(deleteSubscriptionReq, CancellationToken.None);

            //Assert
            Assert.Equal("NDC", result.Channel);
            Assert.Equal("NONPROD", result.Environment);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("Deleted", result.SubscriptionState);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoContentFound()
        {
            //Arrange
            var mockServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NoContent, "Deleted");
            var deleteSubscriptionReq = new DeleteAPIMSubscriptionRepoCommand("URl", new SubscriptionDeleteCommand { SubscriptionId = "Subscription1" });
            _mediator.Setup(m => m.Send(It.IsAny<DeleteAPIMSubscriptionServiceCommand>(), CancellationToken.None)).ReturnsAsync(mockServiceResponse);

            //Act
            DeleteAPIMSubscriptionRepoCommandHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(deleteSubscriptionReq, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.VE002, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.VE002Reason, result.Error.Message);
            Assert.Equal(Status.OK, result.Status);
        }
    }
}
