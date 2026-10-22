using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta; 
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class FlexiDateSearchQueryHandlerTests
    {
        private readonly Mock<ILogger<FlexiDateSearchQueryHandler>> _logger;
        private readonly Mock<IDeltaGqlService> _deltaGqlService;
        private readonly FlexiDateSearchQueryHandler _handler;

        public FlexiDateSearchQueryHandlerTests()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();

            _logger = new Mock<ILogger<FlexiDateSearchQueryHandler>>();
            _deltaGqlService = new Mock<IDeltaGqlService>();
            _handler = new FlexiDateSearchQueryHandler(_deltaGqlService.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlexiDateSearchQueryHandler(null, _logger.Object));
            Assert.Throws<ArgumentNullException>(() => new FlexiDateSearchQueryHandler(_deltaGqlService.Object, null));
        }

        [Fact]
        public async Task Handler_Revenue()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.FlexiDateSearch.FlexiDateOfferRequest>("OfferMS/FlexiDateSearchQuery/Revenue_OfferMsRQ");
            var response = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS/FlexiDateSearchQuery/Revenue_DeltaGqlRS");
            
            _deltaGqlService.Setup(ds => ds.SendAsync(It.IsAny<GqlSearchOffersVariables>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>())).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(new FlexiDateSearchQuery(request), CancellationToken.None);

            // Assert
            Assert.Equal(["2025-03-25", "2025-03-26", "2025-03-27", "2025-03-28", "2025-03-29", "2025-03-30", "2025-03-31"], result.Value.Value.FlexibleDateCalendar.ColumnText);
            
            var firstDepartureRow = result.Value.Value.FlexibleDateCalendar.DepartureDetails.First();
            Assert.Equal(new DateTime(2025,3,12), firstDepartureRow.DepartureDate);

            var firstDepartureCell = firstDepartureRow.FlexibleDatePriceCalendarCell.First();
            Assert.Equal("2025-03-12", firstDepartureCell.OriginDestinations.First().DepartureDate);
            Assert.Equal("LHR", firstDepartureCell.OriginDestinations.First().OriginAirport);
            Assert.Equal("JFK", firstDepartureCell.OriginDestinations.First().DestinationAirport);
            Assert.Equal("2025-03-25", firstDepartureCell.OriginDestinations.Last().DepartureDate);
            Assert.Equal("JFK", firstDepartureCell.OriginDestinations.Last().OriginAirport);
            Assert.Equal("LHR", firstDepartureCell.OriginDestinations.Last().DestinationAirport);
            Assert.Equal("365.22", firstDepartureCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            Assert.Equal("357.22", firstDepartureCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            Assert.Equal(2, firstDepartureCell.SalePrice.Count()); // BasePrice and TotalTax only
            Assert.NotEmpty(firstDepartureCell.OfferId);
            Assert.True(firstDepartureCell.LowestPriceOfferIndicator);

            var fourthDepartureCell = firstDepartureRow.FlexibleDatePriceCalendarCell.ElementAt(3);
            Assert.Equal("2025-03-12", fourthDepartureCell.OriginDestinations.First().DepartureDate);
            Assert.Equal("2025-03-28", fourthDepartureCell.OriginDestinations.Last().DepartureDate);
            Assert.Equal("391.42", fourthDepartureCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            Assert.Equal("361.42", fourthDepartureCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            Assert.False(fourthDepartureCell.LowestPriceOfferIndicator);
        }

        [Fact]
        public async Task Handler_Award()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.FlexiDateSearch.FlexiDateOfferRequest>("OfferMS/FlexiDateSearchQuery/Award_OfferMsRQ");
            var response = TestDataProvider.GetTestData<GraphQLResponse>("OfferMS/FlexiDateSearchQuery/Award_DeltaGqlRS");

            _deltaGqlService.Setup(ds => ds.SendAsync(It.IsAny<GqlSearchOffersVariables>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>())).ReturnsAsync(response);

            // Act
            var result = await _handler.Handle(new FlexiDateSearchQuery(request), CancellationToken.None);

            // Assert
            Assert.Equal(["2025-06-25", "2025-06-26", "2025-06-27", "2025-06-28", "2025-06-29", "2025-06-30", "2025-07-01"], result.Value.Value.FlexibleDateCalendar.ColumnText);

            var firstDepartureRow = result.Value.Value.FlexibleDateCalendar.DepartureDetails.First();
            Assert.Equal(new DateTime(2025, 6, 12), firstDepartureRow.DepartureDate);
            Assert.Empty(firstDepartureRow.FlexibleDatePriceCalendarCell);

            var cheapestCell = result.Value.Value.FlexibleDateCalendar.DepartureDetails.First(dd => dd.DepartureDate == new DateTime(2025, 6, 13)).FlexibleDatePriceCalendarCell.First();
            Assert.Equal("2025-06-13", cheapestCell.OriginDestinations.First().DepartureDate);
            Assert.Equal("LHR", cheapestCell.OriginDestinations.First().OriginAirport);
            Assert.Equal("MLE", cheapestCell.OriginDestinations.First().DestinationAirport);
            Assert.Equal("2025-06-25", cheapestCell.OriginDestinations.Last().DepartureDate);
            Assert.Equal("BOM", cheapestCell.OriginDestinations.Last().OriginAirport);
            Assert.Equal("LHR", cheapestCell.OriginDestinations.Last().DestinationAirport);
            Assert.Equal(82500, cheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.PointsEnum).PointsAmount.PointsCount);
            Assert.Equal("577.42", cheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            Assert.Equal("577.42", cheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            Assert.Equal(3, cheapestCell.SalePrice.Count()); // Points, BasePrice and TotalTax only
            Assert.NotEmpty(cheapestCell.OfferId);
            Assert.True(cheapestCell.LowestPriceOfferIndicator);

            var notCheapestCell = result.Value.Value.FlexibleDateCalendar.DepartureDetails.First(dd => dd.DepartureDate == new DateTime(2025,6,14)).FlexibleDatePriceCalendarCell.First();
            Assert.Equal("2025-06-14", notCheapestCell.OriginDestinations.First().DepartureDate);
            Assert.Equal("2025-06-25", notCheapestCell.OriginDestinations.Last().DepartureDate);
            Assert.Equal(92500, notCheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.PointsEnum).PointsAmount.PointsCount);
            Assert.Equal("582.12", notCheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);
            Assert.Equal("582.12", notCheapestCell.SalePrice.First(sp => sp.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount);
            Assert.False(notCheapestCell.LowestPriceOfferIndicator);
        }
    }
}
