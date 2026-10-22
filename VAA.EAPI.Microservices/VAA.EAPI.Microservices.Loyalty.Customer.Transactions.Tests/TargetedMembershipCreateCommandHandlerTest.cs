using Azure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipCreateCommandHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<TargetedMembershipCommandResponse>>> _mockLogger;
        private readonly TargetedMembershipCreateCommandHandler _handler;

        public TargetedMembershipCreateCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseCommandHandler<TargetedMembershipCommandResponse>>>();
            _handler = new TargetedMembershipCreateCommandHandler(_mockMediator.Object, _mockLogger.Object);
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCreateCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCreateCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipCreateCommand>("TargetedMembership\\TargetedMembershipCreateCommandData");
            var response = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.True(response.Result.IsSuccess);
        }

        [Fact]
        public void HandlerSuccessWithException()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipCreateCommand>("TargetedMembership\\TargetedMembershipCreateCommandData");
            var response = _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}
