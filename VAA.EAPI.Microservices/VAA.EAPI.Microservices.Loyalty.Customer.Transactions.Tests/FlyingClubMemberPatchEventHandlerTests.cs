using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.FlyingClubMember;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class FlyingClubMemberPatchEventHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseEventHandler>> _mockLogger;
        private readonly FlyingClubMemberPatchEventHandler _handler;

        public FlyingClubMemberPatchEventHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseEventHandler>>();
            _handler = new FlyingClubMemberPatchEventHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberPatchEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberPatchEventHandler(_mockMediator.Object, null));
        }
    }
}
