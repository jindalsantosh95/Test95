using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderDataRepoComamandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly OrderDocRepositoryCommandHandler _mockHandler;

        public OrderDataRepoComamandHandlerTests()
        {
            OrderDataTestsCommon.CreateTestEnvironmentSettings();

            _mockMediator = new Mock<IMediator>();
            _mockHandler = new OrderDocRepositoryCommandHandler(_mockMediator.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderDocRepositoryCommandHandler(null));
        }
        [Fact]
        public async Task OrderDocRepositoryCommandHandler_ShouldReturnUnitValue_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OrderDocRepositoryCommand>("PaymentMS\\OrderDocRepositoryCommandData");
            _mockMediator.Setup(m => m.Send(It.IsAny<CosmosModifyRepoCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Equals(Unit.Value));

        }
    }
}
