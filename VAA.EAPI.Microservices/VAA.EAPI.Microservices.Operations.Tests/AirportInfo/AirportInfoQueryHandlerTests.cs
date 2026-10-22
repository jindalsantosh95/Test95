using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Microservices.Operations.Application.Query;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Operations.Tests.AirportInfo
{
    public class AirportInfoQueryHandlerTests
    {
        private readonly Mock<ILogger<AirportInfoQueryHandler>> _logger;
        private readonly Mock<IAirportInfoProvider> _airportInfoProvider;
        private readonly AirportInfoQueryHandler _classUnderTest;

        public AirportInfoQueryHandlerTests()
        {
            _logger = new Mock<ILogger<AirportInfoQueryHandler>>();
            _airportInfoProvider = new Mock<IAirportInfoProvider>();
            _classUnderTest = new AirportInfoQueryHandler(_airportInfoProvider.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullProviders()
        {
            Assert.Throws<ArgumentNullException>(() => new AirportInfoQueryHandler(null, _logger.Object));
            Assert.Throws<ArgumentNullException>(() => new AirportInfoQueryHandler(_airportInfoProvider.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {

            // Arrange
            _airportInfoProvider.Setup(s => s.GetAllAirportsAsync()).ReturnsAsync(new List<Domain.AirportInfo.AirportInfo>());

            // Act
            var testResult = await _classUnderTest.Handle(new AirportInfoQuery(), CancellationToken.None);

            // Assert
            Assert.True(testResult.IsSuccess);

        }
    }
}
