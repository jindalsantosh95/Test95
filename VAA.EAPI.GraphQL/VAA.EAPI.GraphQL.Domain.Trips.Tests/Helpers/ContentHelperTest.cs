using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers
{
    public class ContentHelperTest
    {
        [Fact]
        public async Task GetDestinationImageContent_NullInput_ReturnsEmpty()
        {           
            var result = ContentHelpers.GetDestinationImageContent(null);
            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetDestinationImageContent_destinationCardsEmpty_ReturnsEmpty()
        {
            // Arrange
            string setupJson = "{\"destinationCards\":null}";
            var contentList = new List<string> { setupJson };

            var result = ContentHelpers.GetDestinationImageContent(contentList);
            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetDestinationImageContent_ReturnsContent()
        {
            // Arrange
            string setupJson = "{\"destinationCards\":[{\"iata\":\"DEL\",\"showDestinationCardInExplore\":true,\"countryLabel\":\"India\",\"cityLabel\":\"Delhi\",\"cardOrder\":\"1\",\"destinationImage\":null,\"showFlightPrice\":true,\"gbpFlightPricePp\":\"222\",\"usdFlightPricePp\":\"345\",\"flightLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/asia/india/delhi\",\"showHolidayPrice\":true,\"gbpHolidayPricePp\":\"123\",\"holidayLinkUrl\":\"https://ww.virginatlantic.com/holidays\"},{\"iata\":\"DXB\",\"showDestinationCardInExplore\":true,\"countryLabel\":\"UAE\",\"cityLabel\":\"Dubai\",\"cardOrder\":\"2\",\"destinationImage\":{\"_publishUrl\":\"https://stg-vaabrowse.virginatlantic.com/content/dam/vaa-home/sales-banner-images/dubai-desert-ladies.jpg\"},\"showFlightPrice\":true,\"gbpFlightPricePp\":\"123\",\"usdFlightPricePp\":\"345\",\"flightLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/middle-east/dubai\",\"showHolidayPrice\":true,\"gbpHolidayPricePp\":\"333\",\"holidayLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/middle-east/dubai\"},{\"iata\":\"BGI\",\"showDestinationCardInExplore\":true,\"countryLabel\":\"Barbados\",\"cityLabel\":\"Bridgetown\",\"cardOrder\":\"3\",\"destinationImage\":{\"_publishUrl\":\"https://stg-vaabrowse.virginatlantic.com/content/dam/vaa-home/sales-banner-images/barbados-snorkels.jpg\"},\"showFlightPrice\":true,\"gbpFlightPricePp\":\"123\",\"usdFlightPricePp\":\"333\",\"flightLinkUrl\":\"444\",\"showHolidayPrice\":true,\"gbpHolidayPricePp\":\"121\",\"holidayLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/caribbean/barbados\"},{\"iata\":\"JFK\",\"showDestinationCardInExplore\":true,\"countryLabel\":\"United States\",\"cityLabel\":\"New york\",\"cardOrder\":\"4\",\"destinationImage\":{\"_publishUrl\":\"https://stg-vaabrowse.virginatlantic.com/content/dam/vaa-home/sales-banner-images/las-vegas-skyline-bf-image.jpg\"},\"showFlightPrice\":true,\"gbpFlightPricePp\":\"345\",\"usdFlightPricePp\":\"555\",\"flightLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/north-america/usa/new-york\",\"showHolidayPrice\":true,\"gbpHolidayPricePp\":\"123\",\"holidayLinkUrl\":\"https://www.virginatlantic.com/where-we-fly/north-america/usa/new-york\"}]}";

            var contentList = new List<string> { setupJson };

            var result = ContentHelpers.GetDestinationImageContent(contentList.ToArray());

            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal("DEL", result.First().Key);
            Assert.Empty(result.First().Value);
            Assert.Equal("DXB", result.Skip(1).First().Key);
            Assert.Equal("https://stg-vaabrowse.virginatlantic.com/content/dam/vaa-home/sales-banner-images/dubai-desert-ladies.jpg", result.Skip(1).First().Value);
        }
    }
}
