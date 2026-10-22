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
    public class GroupDocRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly GroupDocRepoQueryHandler _handler;
        private readonly Mock<ILogger<GroupDocRepoQueryHandler>> _logger;

        public GroupDocRepoQueryHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GroupDocRepoQueryHandler>>();
            _handler = new GroupDocRepoQueryHandler(_mockMediator.Object, _logger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupDocRepoQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerTransactionMS\\GroupDocuments");
            string groupId = "1-3ZK-305";
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            GroupDocRepoQuery query = new GroupDocRepoQuery(groupId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
