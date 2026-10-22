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
    public class CustomerTransactionRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();
        private readonly Mock<ILogger<CustomerTransactionRepoQueryHandler>> _mockLogger = new();

        public CustomerTransactionRepoQueryHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionRepoQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionRepoQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodContactNoteRequest()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("ListOfContactNotes");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(m => m.Send(It.IsAny<CosmosReaderRepoQuery>(), CancellationToken.None)).ReturnsAsync(mockResponse);
            CustomerTransactionRepoQuery query = new(customerID, "ContactNote", new List<string>());
            CustomerTransactionRepoQueryHandler handler = new CustomerTransactionRepoQueryHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var results = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<List<object>>(results);

        }

    }
}
