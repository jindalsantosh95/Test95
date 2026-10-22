using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests.Banner
{
    public class PatchBannerCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();
        private readonly Mock<ILogger<PatchBannerCommandHandler>> _mockLogger = new();
        private readonly PatchBannerCommandHandler _handler;

        public PatchBannerCommandHandlerTests()
        {
            _handler = new PatchBannerCommandHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PatchBannerCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PatchBannerCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnAccepted_GoodPATCHRequest()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()));
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()));
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var command = TestDataProvider.GetTestData<PatchBannerCommand>("CustomerTransactionMS\\PatchBannerCommandRequest");
            command.CustomerId = customerID;

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Status.Accepted, result.Value.Status);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal("location", result.Value.HeaderParameters.First().Key);
            Assert.Contains("Banners/status", result.Value.HeaderParameters.First().Value);
        }
    }
}
