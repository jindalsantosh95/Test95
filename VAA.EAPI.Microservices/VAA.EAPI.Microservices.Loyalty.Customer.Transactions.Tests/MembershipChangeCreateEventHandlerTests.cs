using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MembershipChange;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class MembershipChangeCreateEventHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseEventHandler>> _mockLogger;
        private readonly MembershipChangeCreateEventHandler _handler;

        public MembershipChangeCreateEventHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseEventHandler>>();
            _handler = new MembershipChangeCreateEventHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MembershipChangeCreateEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MembershipChangeCreateEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void Handler_Success_With_No_Error()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            var eventRequest = TestDataProvider.GetTestData<MembershipChangeCreateEvent>("MembershipChanges\\MembershipChangeCreateEvent");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
        }

        [Fact]
        public void Handler_Success_With_Exception()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            var eventRequest = TestDataProvider.GetTestData<MembershipChangeCreateEvent>("MembershipChanges\\MembershipChangeCreateEvent");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.NotEmpty(response.Exception.InnerExceptions);
        }
    }
}
