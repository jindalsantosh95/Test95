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
    public class PnrSearchDocRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PnrSearchDocRepositoryCommandHandler _mockHandler;

        public PnrSearchDocRepoCommandHandlerTests()
        {
            OrderDataTestsCommon.CreateTestEnvironmentSettings();

            _mockMediator = new Mock<IMediator>();
            _mockHandler = new PnrSearchDocRepositoryCommandHandler(_mockMediator.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PnrSearchDocRepositoryCommandHandler(null));
        }

        //[Fact]
        //public async Task OrderCreateCommandHandler_ShouldReturnFailure_WhenExceptionOccurs()
        //{
        //    var result = await _mockHandler.Handle(new PnrSearchDocRepositoryCommand(), CancellationToken.None);

        //    Assert.True(result.Equals(false));
        //}#
        [Fact]
        public async Task PnrSearchDocRepositoryCommandHandler_ShouldReturnUnitValue_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PnrSearchDocRepositoryCommand>("PaymentMS\\OrderDocRepositoryCommandData");
            _mockMediator.Setup(m => m.Send(It.IsAny<CosmosModifyRepoCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Equals(Unit.Value));

        }
    }
}
