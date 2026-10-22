using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class MembershipChangeCreateCommandHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<MembershipChangeCommandResponse>>> _mockLogger = new();

        public MembershipChangeCreateCommandHandlerTests()
        {
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MembershipChangeCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MembershipChangeCreateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnAccepted_GoodRequest()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));
            var command = TestDataProvider.GetTestData<MembershipChangeCreateCommand>("MembershipChanges\\MembershipChangeCreateData");
            var handler = new MembershipChangeCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(Status.Accepted, result.Value.Status);
        }

        [Fact]
        public async Task Handle_ShouldThrowBusinessException_EventStoreFailure()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));
            //  string customerID = TestDataProvider.GetTestData<string>("CustID");
            var command = TestDataProvider.GetTestData<MembershipChangeCreateCommand>("MembershipChanges\\MembershipChangeCreateData");
            //  command.CustomerId = customerID;
            var handler = new MembershipChangeCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handle_ShouldThrowBusinessException_CloudEventStoreFailure()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var command = TestDataProvider.GetTestData<MembershipChangeCreateCommand>("MembershipChanges\\MembershipChangeCreateData");
            var handler = new MembershipChangeCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            // Assert
            Assert.True(result.IsFailure);
        }
    }
}

