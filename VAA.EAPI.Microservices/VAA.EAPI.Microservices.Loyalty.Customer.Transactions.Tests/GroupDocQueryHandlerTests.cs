using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class GroupDocQueryHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseQueryHandler<GroupDocumentResponse>>> _mockLogger = new();
        public GroupDocQueryHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed_GoodGroupID()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerTransactionMS\\GroupDocuments");
            string groupId = TestDataProvider.GetTestData<string>("CustomerTransactionMS\\GroupID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            GroupDocQueryHandler handler = new GroupDocQueryHandler(_mockMediator.Object, _mockLogger.Object);

            GroupDocQuery query = new GroupDocQuery(groupId);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<GroupDocumentResponse>(result.Value.Value);
            Assert.Single(result.Value.Value.GroupDocuments);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_Error()
        {
            // Arrange
            string groupId = TestDataProvider.GetTestData<string>("CustomerTransactionMS\\GroupID");
            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GroupDocQueryHandler handler = new GroupDocQueryHandler(_mockMediator.Object, _mockLogger.Object);

            GroupDocQuery query = new GroupDocQuery(groupId);
            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }
    }
}


