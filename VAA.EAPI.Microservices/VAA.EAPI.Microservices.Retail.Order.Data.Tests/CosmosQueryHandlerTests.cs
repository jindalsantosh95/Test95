using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class CosmosQueryHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly CosmosQueryHandler _handler;

        public CosmosQueryHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new CosmosQueryHandler(_mediator.Object);
            OrderDataTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CosmosQueryHandler(null));
        }
        [Fact]
        public async Task CosmosQueryHandler_ShouldSucceed_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CosmosQueryCommand>("PaymentMS\\CosmosQueryCommandData");
            var response = new List<object>()
            {
             "Customer1"
            };

            _mediator.Setup(m => m.Send(It.IsAny<CosmosReaderServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True((string)result.FirstOrDefault() == "Customer1");
        }

    }
}
