using Microsoft.Extensions.Logging;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class BidForUpgradeOffersServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IBidForUpgradeOffersService _mockBidForUpgradeOffersService;

        public BidForUpgradeOffersServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockBidForUpgradeOffersService = new BidForUpgradeOffersService(_httpConnector);
            TripMSEnvironmentSetup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task BidForUpgradeOffers_ShouldSucceed()
        {
            // Arrange
            var request = new BidForUpgradeRequest() { Pnr = "ABC123", LastName = "surName" };

            //Act
            var response = await _mockBidForUpgradeOffersService.GetBidForUpgradeOffersAsync(request);

            //Assert
            Assert.Equal("ELIGIBLE", response.ResponseStatus);
            Assert.Equal("https://canvas-stg.stage.plusgrade.com/offers/airline/partner/partenrid/G5n7IgHx7EVjvsEXePRk26WdCUrrjfxIJs", response.OfferUrl);
        }
    }
}