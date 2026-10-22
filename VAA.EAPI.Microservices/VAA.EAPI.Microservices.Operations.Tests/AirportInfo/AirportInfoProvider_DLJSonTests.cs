using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Operations.Infrastructure.Services.DLJson;

namespace VAA.EAPI.Microservices.Operations.Tests.AirportInfo
{
    public class AirportInfoProvider_DLJSonTests
    {
        private readonly Mock<ILogger<IAirportInfoProvider>> _logger;
        private readonly Mock<IAirportInfoProvider> _airportInfoProvider;
        private readonly AirportInfoProvider_DLJSon _classUnderTest;
        private readonly IMemoryCache _memCache;
        private readonly MockFileSystem _fileSystem;

        public AirportInfoProvider_DLJSonTests()
        {
            _logger = new Mock<ILogger<IAirportInfoProvider>>();
            _airportInfoProvider = new Mock<IAirportInfoProvider>();
            _memCache = new MemoryCache(new MemoryCacheOptions());
            _fileSystem = new MockFileSystem();
            _classUnderTest = new AirportInfoProvider_DLJSon(_logger.Object, _memCache, _fileSystem.FileSystem);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullProviders()
        {
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLJSon(null, _memCache, _fileSystem.FileSystem));
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLJSon(_logger.Object, null, _fileSystem.FileSystem));
            Assert.Throws<ArgumentNullException>(() => new AirportInfoProvider_DLJSon(_logger.Object, _memCache, null));
        }

        [Fact]
        public async Task GetAllAirportsAsync_ShouldSucceed_FromFile()
        {

            // Arrange
            var dataContent = Framework.Tests.TestHelpers.TestDataProvider.GetTestData<string>("Operations\\ListOfAirports");
            _fileSystem.AddFile(@"Infrastructure\Repository\AirportInfoList.json", new MockFileData(dataContent));

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


