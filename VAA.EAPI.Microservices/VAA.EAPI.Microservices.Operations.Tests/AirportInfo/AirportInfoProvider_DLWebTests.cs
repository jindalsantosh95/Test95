using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Operations.Infrastructure.Entities.AirportInfo_DLJson;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Operations.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Operations.Tests.AirportInfo
{
    public class AirportInfoProvider_DLWebTests
    {
        private readonly Mock<ILogger<IAirportInfoProvider>> _logger;
        private readonly Mock<IAirportInfoProvider> _airportInfoProvider;
        private readonly AirportInfoProvider_DLWeb _classUnderTest;
        private readonly IMemoryCache _memCache;
        private readonly Mock<IHttpConnector> _apiConnector;

        public AirportInfoProvider_DLWebTests()
        {
            _logger = new Mock<ILogger<IAirportInfoProvider>>();
            _airportInfoProvider = new Mock<IAirportInfoProvider>();
            _memCache = new MemoryCache(new MemoryCacheOptions());
            _apiConnector = new Mock<IHttpConnector>();
            _classUnderTest = new AirportInfoProvider_DLWeb(_logger.Object, _memCache, _apiConnector.Object);
            OperationsTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullProviders()
        {
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLWeb(null, _memCache, _apiConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLWeb(_logger.Object, null, _apiConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLWeb(_logger.Object, _memCache, null));
        }

        [Fact]
        public async Task GetAllAirportsAsync_ShouldSucceed_FromWeb()
        {
            // Arrange
            var dataContent = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<ListOfCities>("Operations\\ListOfAirports");
            _apiConnector.Setup(c => c.GetAsync<ListOfCities>(It.IsAny<string>(),
                                                 It.IsAny<Dictionary<string, string>>(),
                                                 It.IsAny<string>())).ReturnsAsync(dataContent);

            // Act
            var testOutput = await _classUnderTest.GetAllAirportsAsync();

            // Assert
            Assert.IsType<List<Domain.AirportInfo.AirportInfo>>(testOutput);
            Assert.True(testOutput.Count == 2); // Number of items in Test Data file 
        }

        [Fact]
        public async Task GetAllAirportsAsync_ShouldSucceed_FromCache()
        {

            // Arrange
            //var dataContent = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<ListOfCities>("Operations\\ListOfAirports");
            var dataContent = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<ICollection<Domain.AirportInfo.AirportInfo>>("Operations\\AirportInfo");
            _memCache.Set(Constants.Application.AirportInfoCacheKey, dataContent);

            // Act
            var testOutput = await _classUnderTest.GetAllAirportsAsync();

            // Assert
            Assert.IsType<List<Domain.AirportInfo.AirportInfo>>(testOutput);
            Assert.True(testOutput.Count == 2); // Number of items in Test Data file 
        }
    }
}


