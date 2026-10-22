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
    public class FCMDocBySSANumberRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly FCMDocBySSANumberRepoQueryHandler _handler;
        private readonly Mock<ILogger<FCMDocBySSANumberRepoQueryHandler>> _logger;

        public FCMDocBySSANumberRepoQueryHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<FCMDocBySSANumberRepoQueryHandler>>();
            _handler = new FCMDocBySSANumberRepoQueryHandler(_mockMediator.Object, _logger.Object);
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FCMDocBySSANumberRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FCMDocBySSANumberRepoQueryHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var mockResponse = TestDataProvider.GetTestData<List<object>>("FlyingClubMember\\ListOfFlyingClubMembers");
            string ssaMemberId = "1058255355";
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            FCMDocBySSANumberRepoQuery query = new FCMDocBySSANumberRepoQuery(ssaMemberId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
