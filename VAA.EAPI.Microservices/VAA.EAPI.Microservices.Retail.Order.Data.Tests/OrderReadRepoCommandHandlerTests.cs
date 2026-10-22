using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderReadRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<OrderReadRepositoryCommandHandler>> _mockLogger;
        private readonly OrderReadRepositoryCommandHandler _mockHandler;
        public OrderReadRepoCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<OrderReadRepositoryCommandHandler>>();
            _mockHandler = new OrderReadRepositoryCommandHandler(_mockMediator.Object, _mockLogger.Object);

            OrderDataTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReadRepositoryCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderReadRepositoryCommandHandler(_mockMediator.Object, null));
        }
        [Fact]
        public async Task OrderReadRepositoryCommandHandler_ShouldSuceed_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OrderReadRepositoryCommand>("PaymentMS\\OrderReadRepositoryCommandData");
            var response = new List<object>()
            {

            };

            _mockMediator.Setup(m => m.Send(It.IsAny<CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            //Assert.True(((PnrSearch)result.FirstOrDefault()).OrderRef == "test");

        }
    }

}
