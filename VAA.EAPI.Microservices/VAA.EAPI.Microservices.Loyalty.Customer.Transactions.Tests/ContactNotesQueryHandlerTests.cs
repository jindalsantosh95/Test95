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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ContactNotesQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<ContactNotesResponse>>> _mockLogger = new();

        public ContactNotesQueryHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNotesQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNotesQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodCustomerID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("ListOfContactNotes");
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            ContactNotesQueryHandler handler = new ContactNotesQueryHandler(_mockMediator.Object, _mockLogger.Object);

            ContactNotesQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<ContactNotesResponse>(result.Value.Value);
            Assert.Equal(2, result.Value.Value.ContactNotes.Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorResponse_TransactionRepoError()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            ContactNotesQueryHandler handler = new ContactNotesQueryHandler(_mockMediator.Object, _mockLogger.Object);

            ContactNotesQuery query = new() { CustomerId = customerID };
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.True(result.Value.Errors.Count >= 1);
        }

    }
}
