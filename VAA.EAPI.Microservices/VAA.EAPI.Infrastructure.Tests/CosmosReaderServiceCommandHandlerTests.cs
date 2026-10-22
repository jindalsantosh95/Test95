using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDB;

namespace VAA.EAPI.Infrastructure.Tests
{
    public class CosmosReaderServiceCommandHandlerTests
    {
        private readonly Mock<ILogger<CosmosReaderServiceCommandHandler>> _mockLogger;
        private readonly Mock<IMediator> _mockMediator;
        private readonly CosmosReaderServiceCommandHandler _handler;
        private readonly Mock<CosmosClient> _mockCosmosSQLClient;

        public CosmosReaderServiceCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CosmosReaderServiceCommandHandler>>();
            _mockCosmosSQLClient = new Mock<CosmosClient>();
            _handler = new CosmosReaderServiceCommandHandler(_mockCosmosSQLClient.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenSQLClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CosmosReaderServiceCommandHandler(null, _mockLogger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CosmosReaderServiceCommandHandler(_mockCosmosSQLClient.Object, null));
        }
    }
}
