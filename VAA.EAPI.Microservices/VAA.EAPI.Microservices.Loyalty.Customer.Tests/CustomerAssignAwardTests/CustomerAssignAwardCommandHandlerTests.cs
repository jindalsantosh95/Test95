using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerAssignAwardTests
{
    public class CustomerAssignAwardCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerAssignAwardCommandHandler>> _mockLogger;
        private readonly CustomerAssignAwardCommandHandler _handler;

        public CustomerAssignAwardCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            // Assumes the SEND method always returns TRUE (The actual response is not captured)
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mockLogger = new Mock<ILogger<CustomerAssignAwardCommandHandler>>();
            _handler = new CustomerAssignAwardCommandHandler(_mockMediator.Object, _mockLogger.Object);

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CustomerAssignAwardCommandHandler(_mockMediator.Object, null));
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
        public async Task Handle_ShouldReturnAcceptedStatusWithGoodRequest()
        {
            // Arrange
            var request = new CustomerAssignAwardCommand()
            {
                CloudEvent = new CloudEvent()
                {
                    Data = "12",
                    Id = "123",
                    Source = "xyz",
                    Type = "abc",
                    Subject = "abc"
                }
            };
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Response<string>>>(result);
            Assert.Equal(Status.Accepted, result.Value.Status);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailureWhenExceptionOccurs()
        {
            // Arrange
            var request = new CustomerAssignAwardCommand()
            {
                CloudEvent = new EAPI.Domain.Events.CloudEvent()
                {
                    Data = "12",
                    Id = "123",
                    Source = "xyz",
                    Type = "abc",
                    Subject = "abc"
                }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new NullReferenceException());
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Response<string>>>(result);
            Assert.True(result.IsFailure);
        }
    }
}
