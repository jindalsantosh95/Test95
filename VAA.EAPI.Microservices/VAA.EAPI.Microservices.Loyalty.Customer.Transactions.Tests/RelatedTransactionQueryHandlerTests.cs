using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class RelatedTransactionQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<TransactionResponse>>> _mockLogger = new();

        public RelatedTransactionQueryHandlerTests()
        {
            CampaignTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RelatedTransactionQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RelatedTransactionQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandlerSuccess()
        {
            // Arrange
            var response = TestDataProvider.GetTestData<TransactionResponse>("Transaction\\RelatedTransactionToCancelResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            RelatedTransactionQuery query = new() { CustomerId = customerID, TransactionId = "172848693573060605" };

            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), CancellationToken.None)).ReturnsAsync(new Response<TransactionResponse> { Status = Status.OK, Value = response });
            
            RelatedTransactionQueryHandler handler = new RelatedTransactionQueryHandler(_mockMediator.Object, _mockLogger.Object);
            
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<TransactionResponse>(result.Value.Value);
            Assert.NotEmpty(result.Value.Value.TransactionDocuments);
            Assert.Equal("172848693573060604", result.Value.Value.TransactionDocuments.FirstOrDefault().TransactionId);
            Assert.Equal("172848693573060605", result.Value.Value.TransactionDocuments.FirstOrDefault().ParentTransactionId);
        }

        [Fact]
        public async Task HandlerFailure()
        {
            // Arrange
            var response = TestDataProvider.GetTestData<TransactionResponse>("Transaction\\RelatedTransactionToCancelResponse");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            RelatedTransactionQuery query = new() { CustomerId = customerID, TransactionId = "172848693573060603" };

            _mockMediator.Setup(m => m.Send(It.IsAny<TransactionQuery>(), CancellationToken.None)).ReturnsAsync(new Response<TransactionResponse> { Status = Status.OK, Value = response });

            RelatedTransactionQueryHandler handler = new RelatedTransactionQueryHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<List<ErrorDetails>>(result.Value.Errors);
            Assert.NotEmpty(result.Value.Errors);
            Assert.Equal("TransactionID not found", result.Value.Errors.FirstOrDefault().Message);
            Assert.Equal("800", result.Value.Errors.FirstOrDefault().Code);
        }
    }
}
