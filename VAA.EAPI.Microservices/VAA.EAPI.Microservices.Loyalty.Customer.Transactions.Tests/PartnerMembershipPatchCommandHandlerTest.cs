using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PartnerMembershipPatchCommandHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<PartnerMembershipCommandResponse>>> _mockLogger;
        private readonly PartnerMembershipPatchCommandHandler _handler;

        public PartnerMembershipPatchCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseCommandHandler<PartnerMembershipCommandResponse>>>();
            _handler = new PartnerMembershipPatchCommandHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipPatchCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipPatchCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<PartnerMembershipPatchCommand>("PartnerMembership\\PartnerMembershipPatchCommandData");

            //Act
            var response = _handler.Handle(eventRequest, CancellationToken.None).Result;

            //Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Value.HeaderParameters);
            Assert.Equal("location", response.Value.HeaderParameters[0].Key);
            Assert.NotNull(response.Value.HeaderParameters[0].Value);
            Assert.Contains("http://localhost:7071/api/PartnerMemberships/status/", response.Value.HeaderParameters[0].Value);
        }
    }
}

