using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.TargetedMemberships;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipCreateEventHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseEventHandler>> _mockLogger;
        private readonly TargetedMembershipCreateEventHandler _handler;

        public TargetedMembershipCreateEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseEventHandler>>();
            _handler = new TargetedMembershipCreateEventHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCreateEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCreateEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipCreateEvent>("TargetedMembership\\TargertedMembershipCreateEventData");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
        }

        [Fact]
        public void HandlerSuccessWithException()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipCreateEvent>("TargetedMembership\\TargertedMembershipCreateEventData");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.NotEmpty(response.Exception.InnerExceptions);
        }

    }
}
