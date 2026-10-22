using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class LoyaltyCustomerAssignAwardCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<LoyaltyCustomerAssignAwardCommandHandler>> _mockLogger;
        private readonly LoyaltyCustomerAssignAwardCommandHandler _handler;

        public LoyaltyCustomerAssignAwardCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mockLogger = new Mock<ILogger<LoyaltyCustomerAssignAwardCommandHandler>>();
            _handler = new LoyaltyCustomerAssignAwardCommandHandler(_mockMediator.Object, _mockLogger.Object);
            CustomerTestsCommon.CreateTestEnvironmentSettings();

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltyCustomerAssignAwardCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltyCustomerAssignAwardCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldThrowNullExceptionWhenRequestIsNull()
        {
            // Act
            async Task act() => await _handler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public async Task Handle_ShouldReturnAcceptedStatusWithNoException()
        {

            // Act
            var result = await _handler.Handle(new LoyaltyCustomerAssignAwardCommand(), CancellationToken.None);

            // Assert

            Assert.Equal(Status.Accepted, result.Value.Status);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailureWhenExceptionOccursWhileStoring()
        {
            //Arrange

            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new NullReferenceException());

            // Act

            var result = await _handler.Handle(new LoyaltyCustomerAssignAwardCommand(), CancellationToken.None);

            // Assert

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailureWhenExceptionOccursWhilePublishing()
        {
            //Arrange

            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NullReferenceException());

            // Act

            var result = await _handler.Handle(new LoyaltyCustomerAssignAwardCommand(), CancellationToken.None);

            // Assert

            Assert.True(result.IsFailure);
        }
    }
}
