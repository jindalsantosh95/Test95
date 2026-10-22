using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PartnerMembershipQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<PartnerMembershipResponse>>> _mockLogger = new();

        public PartnerMembershipQueryHandlerTest()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerMembershipQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodCustomerID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("PartnerMembership\\ListOfPartnerMembership");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            PartnerMembershipQueryHandler handler = new PartnerMembershipQueryHandler(_mockMediator.Object, _mockLogger.Object);

            PartnerMembershipQuery query = new PartnerMembershipQuery();
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<PartnerMembershipResponse>(result.Value.Value);
            Assert.Single(result.Value.Value.PartnerMembershipDocuments);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_Error()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            PartnerMembershipQueryHandler handler = new PartnerMembershipQueryHandler(_mockMediator.Object, _mockLogger.Object);

            PartnerMembershipQuery query = new PartnerMembershipQuery();
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
