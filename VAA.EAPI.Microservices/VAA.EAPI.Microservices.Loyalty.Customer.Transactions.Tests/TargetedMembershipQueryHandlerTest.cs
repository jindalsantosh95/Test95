using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<TargetedMembershipResponse>>> _mockLogger = new();
        public TargetedMembershipQueryHandlerTest()
        {
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedGoodCustomerID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("TargetedMembership\\ListOfTargetedMembership");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            TargetedMembershipQueryHandler handler = new TargetedMembershipQueryHandler(_mockMediator.Object, _mockLogger.Object);

            TargetedMembershipQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<TargetedMembershipResponse>(result.Value.Value);

        }

        [Fact]
        public async Task HandleShouldSucceedError()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            TargetedMembershipQueryHandler handler = new TargetedMembershipQueryHandler(_mockMediator.Object, _mockLogger.Object);

            TargetedMembershipQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }


    }
}
