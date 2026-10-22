using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class CustomerAssignAwardEventProcessCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerAssignAwardEventProcessCommandHandler>> _mockLogger;
        private readonly CustomerAssignAwardEventProcessCommandHandler _handler;

        public CustomerAssignAwardEventProcessCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            // Assumes the SEND method always returns TRUE (The actual response is not captured)
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerAssignAwardRepoCommand>(), It.IsAny<CancellationToken>())).Equals(true);

            _mockLogger = new Mock<ILogger<CustomerAssignAwardEventProcessCommandHandler>>();
            _handler = new CustomerAssignAwardEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object);

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardEventProcessCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardEventProcessCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handle_ShouldThrowNullExceptionWhenRequestIsNull()
        {
            // Arrange

            // Act
            async Task act() => await _handler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalseWhenDataIsEmpty()
        {
            // Arrange

            var request = new CustomerAssignAwardEventProcessCommand()
            {
                Data = new AssignAwardRequestData()
            };
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}
