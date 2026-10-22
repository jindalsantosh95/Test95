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
    public class GroupDocByHeadCustomerIdRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly GroupDocByHeadCustomerIdRepoQueryHandler _handler;
        private readonly Mock<ILogger<GroupDocByHeadCustomerIdRepoQueryHandler>> _logger;

        public GroupDocByHeadCustomerIdRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GroupDocByHeadCustomerIdRepoQueryHandler>>();
            _handler = new GroupDocByHeadCustomerIdRepoQueryHandler(_mockMediator.Object, _logger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocByHeadCustomerIdRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocByHeadCustomerIdRepoQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerTransactionMS\\GroupDocuments");
            string headCustomerId = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            GroupDocByHeadCustomerIdRepoQuery query = new GroupDocByHeadCustomerIdRepoQuery(headCustomerId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}

