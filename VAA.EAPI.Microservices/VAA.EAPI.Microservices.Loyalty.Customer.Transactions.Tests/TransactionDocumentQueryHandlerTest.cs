using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TransactionDocumentQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<TransactionResponse>>> _mockLogger = new();
        public TransactionDocumentQueryHandlerTest()
        { }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionDocumentQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionDocumentQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<TransactionDocument>>("AccountDetails\\ListOfTransactions");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            TransactionDocumentQueryHandler handler = new TransactionDocumentQueryHandler(_mockMediator.Object, _mockLogger.Object);

            TransactionQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<TransactionResponse>(result.Value.Value);
            Assert.NotEmpty(result.Value.Value.TransactionDocuments);
        }

        [Fact]
        public async Task HandleShouldThrowException()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            TransactionDocumentQueryHandler handler = new TransactionDocumentQueryHandler(_mockMediator.Object, _mockLogger.Object);

            TransactionQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
