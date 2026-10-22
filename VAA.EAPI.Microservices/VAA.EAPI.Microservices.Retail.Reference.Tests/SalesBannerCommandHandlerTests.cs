using Azure.Core;
using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class SalesBannerCommandHandlerTests
    {
        private readonly Mock<IDistributedCacheService> _cache;
        private readonly SalesBannersCommandHandler _handler;
        private readonly Mock<IMediator> _mediator;

        public SalesBannerCommandHandlerTests()
        {
            _cache = new Mock<IDistributedCacheService>();
            _mediator = new Mock<IMediator>();
            _cache.Setup(m => m.Get<string>("sales-banners:seasonal-messaging")).ReturnsAsync(TestDataProvider.GetTestData<string>("ReferenceMS\\CachedSalesBanners-SeasonalMessaging"));
            _cache.Setup(m => m.Get<string>("sales-banners:search-top")).ReturnsAsync(TestDataProvider.GetTestData<string>("ReferenceMS\\CachedSalesBanners-SearchTop"));
            _handler = new SalesBannersCommandHandler(_cache.Object, _mediator.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SalesBannersCommandHandler(null, null));
        }

        [Fact]
        public async Task MultipleDisplayLocations_BothMatchAndCorrectlyOrdered()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top", "seasonal-messaging" },
                    Request = new()
                    {
                        BookingType = "flight-only",
                        OriginAirport = "LHR"
                    }
                }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for each display location, containing the matching test sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(1, result["search-top"]?.Count());
            Assert.True(result.ContainsKey("seasonal-messaging"));
            Assert.Equal(4, result["seasonal-messaging"]?.Count());
            Assert.Equal(2, result.Count);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal(result["search-top"].Min(b => b.Ranking), result["search-top"].First().Ranking);
            Assert.Equal(result["search-top"].Max(b => b.Ranking), result["search-top"].Last().Ranking);
        }

        [Fact]
        public async Task SalesBanner_NoMatch()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "LGW",
                        DestinationAirport = "LHR"
                    }
                }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing empty list of banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(0, result["search-top"]?.Count());
            Assert.Single(result);
        }

        [Fact]
        public async Task SalesBanner_DepartureAirportOnly()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "LHR",
                    }
                }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(1, result["search-top"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal("Start your journey in style at the Virgin Atlantic Clubhouse", result["search-top"].First().Title);
        }

        [Fact]
        public async Task SalesBanner_DestinationAirport()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "MAN",
                        DestinationAirport = "LAS"
                    }
                }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(2, result["search-top"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal("Did you know?", result["search-top"].First().Title);
            Assert.Equal("Discover Las Vegas", result["search-top"].Last().Title);
        }

        [Fact]
        public async Task SeasonalMessaging_BookingType_NullRanking()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "seasonal-messaging" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        BookingType = "flight-only"
                    }
                }
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching flight-only seasonal messages. No other locations returned.
            Assert.True(result.ContainsKey("seasonal-messaging"));
            Assert.Equal(3, result["seasonal-messaging"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking, with null first
            Assert.Equal("No Route Null", result["seasonal-messaging"].ElementAt(0).Title);
            Assert.Equal("No Route Seven", result["seasonal-messaging"].ElementAt(1).Title);
            Assert.Equal("No Route Twelve", result["seasonal-messaging"].ElementAt(2).Title);
        }
    }
}