using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class AwardPatchEventHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseEventHandler>> _mockLogger;
        private readonly AwardPatchEventHandler _handler;

        public AwardPatchEventHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseEventHandler>>();
            _handler = new AwardPatchEventHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardPatchEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardPatchEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            var mockResponsAwards = TestDataProvider.GetTestData<Response<AwardResponse>>("CustomerTransactionMS\\AwardsList");
            _mockMediator.Setup(m => m.Send(It.IsAny<AwardsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponsAwards);

            var eventRequest = TestDataProvider.GetTestData<AwardPatchEvent>("CustomerTransactionMS\\AwardPatchEventData");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);

            //Assert
            Assert.True(response.Result.IsSuccess);
            Assert.False(response.Result.IsFailure);
        }
    }
}
