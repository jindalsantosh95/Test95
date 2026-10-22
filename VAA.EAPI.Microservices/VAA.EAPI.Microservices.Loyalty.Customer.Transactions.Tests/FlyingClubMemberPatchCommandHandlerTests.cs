using Azure.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class FlyingClubMemberPatchCommandHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<FlyingClubMemberCommandResponse>>> _mockLogger;
        private readonly FlyingClubMemberPatchCommandHandler _handler;

        public FlyingClubMemberPatchCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseCommandHandler<FlyingClubMemberCommandResponse>>>();
            _handler = new FlyingClubMemberPatchCommandHandler(_mockMediator.Object, _mockLogger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberPatchCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlyingClubMemberPatchCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));

            var eventRequest = TestDataProvider.GetTestData<FlyingClubMemberPatchCommand>("FlyingClubMember\\FlyingClubMemberPatchCommandData");

            //Act
            var response = _handler.Handle(eventRequest, CancellationToken.None).Result;

            //Assert
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Value.HeaderParameters);
            Assert.Equal("location", response.Value.HeaderParameters[0].Key);
            Assert.NotNull(response.Value.HeaderParameters[0].Value);
            Assert.Contains("http://localhost:7071/api/FlyingClubMembers/status/", response.Value.HeaderParameters[0].Value);
        }
    }
}


