using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.PaymentUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class PaymentUpdateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PaymentUpdateCommandHandler>> _mockLogger;
        private readonly PaymentUpdateCommandHandler _mockHandler;

        public PaymentUpdateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PaymentUpdateCommandHandler>>();
            _mockHandler = new PaymentUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task PaymentUpdateCommandHandler_ShouldReturnFailure_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _mockHandler.Handle(new PaymentUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task PaymentUpdateCommandHandler_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                        .Equals(true);

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task PaymentUpdateCommandHandler_ShouldReturnAcceptedStatus_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PaymentUpdateCommand>("PaymentMS\\OrderPaymentCommandRequest");

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
    }
}
