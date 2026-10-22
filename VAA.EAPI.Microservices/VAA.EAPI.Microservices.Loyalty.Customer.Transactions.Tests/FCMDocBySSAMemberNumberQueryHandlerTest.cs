using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class FCMDocBySSAMemberNumberQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<FlyingClubMemberResponse>>> _mockLogger = new();

        public FCMDocBySSAMemberNumberQueryHandlerTest()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FCMDocBySSAMemberNumberQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FCMDocBySSAMemberNumberQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodSSMMemberId()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("FlyingClubMember\\ListOfFlyingClubMembers");
            string ssaMemberId = "1058255355";
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            FCMDocBySSAMemberNumberQueryHandler handler = new FCMDocBySSAMemberNumberQueryHandler(_mockMediator.Object, _mockLogger.Object);

            FCMDocBySSAMemberNumberQuery query = new FCMDocBySSAMemberNumberQuery(ssaMemberId);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<FlyingClubMemberResponse>(result.Value.Value);
            Assert.Single(result.Value.Value.FlyingClubMemberDocuments);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_Error()
        {
            // Arrange
            string ssaMemberId = "1058255355";
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            FCMDocBySSAMemberNumberQueryHandler handler = new FCMDocBySSAMemberNumberQueryHandler(_mockMediator.Object, _mockLogger.Object);

            FCMDocBySSAMemberNumberQuery query = new FCMDocBySSAMemberNumberQuery(ssaMemberId);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
