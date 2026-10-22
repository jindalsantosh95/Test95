using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class GroupDocFromHistoryByCustomerIdRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly GroupDocFromHistoryByCustomerIdRepoQueryHandler _handler;
        private readonly Mock<ILogger<GroupDocFromHistoryByCustomerIdRepoQueryHandler>> _logger;

        public GroupDocFromHistoryByCustomerIdRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GroupDocFromHistoryByCustomerIdRepoQueryHandler>>();
            _handler = new GroupDocFromHistoryByCustomerIdRepoQueryHandler(_mockMediator.Object, _logger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocFromHistoryByCustomerIdRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocFromHistoryByCustomerIdRepoQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerTransactionMS\\GroupDocuments");
            string customerId = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            GroupDocFromHistoryByCustomerIdRepoQuery query = new GroupDocFromHistoryByCustomerIdRepoQuery(customerId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}

