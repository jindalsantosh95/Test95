using Azure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class SFPDSyncCommandHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<SFPDSyncCommandHandler>> _mockLogger;
        private readonly SFPDSyncCommandHandler _handler;

        public SFPDSyncCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<SFPDSyncCommandHandler>>();
            _handler = new SFPDSyncCommandHandler(_mockLogger.Object, _mockMediator.Object);
            LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SFPDSyncCommandHandler(_mockLogger.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SFPDSyncCommandHandler(null, _mockMediator.Object));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);

            var eventRequest = TestDataProvider.GetTestData<SFPDSyncCommand>("LoyaltySyncMS\\SFPDSyncRequest");

            //Act
            var response = _handler.Handle(eventRequest, CancellationToken.None).Result;

            //Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Value.HeaderParameters);
            Assert.Equal("location", response.Value.HeaderParameters[0].Key);
            Assert.NotNull(response.Value.HeaderParameters[0].Value);
            Assert.Contains("http://localhost:7075/api/SyncSFPD/status", response.Value.HeaderParameters[0].Value);
        }
    }
}
