using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Activity;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ActivityQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<ActivitiesResponse>>> _mockLogger = new();

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivitiesQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivitiesQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_ForGoodRequest()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("Activity\\ActivityDoc");
            string customerID = TestDataProvider.GetTestData<string>("CustID");

            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            ActivitiesQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);

            ActivitiesQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<ActivitiesResponse>(result.Value.Value);
            Assert.Equal(2, result.Value.Value.Activities.Count());
        }

        [Fact]
        public async Task Handle_ShouldThrowError_ForDBExceptions()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            ActivitiesQueryHandler handler = new(_mockMediator.Object, _mockLogger.Object);
            ActivitiesQuery query = new() { CustomerId = customerID };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
