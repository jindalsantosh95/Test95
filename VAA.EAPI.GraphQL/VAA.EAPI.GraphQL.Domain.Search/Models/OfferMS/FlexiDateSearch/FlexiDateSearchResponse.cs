using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch
{
    public class FlexiDateSearchResponse
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public FlexibleDateCalendar FlexibleDateCalendar { get; set; }
        public bool isAwardSearch => (FlexibleDateCalendar?.DepartureDetails?.Any(details => details.FlexibleDatePriceCalendarCell.Any(price => price.SalePrice?.FirstOrDefault(price => price?.PriceComponentType == Price.PriceComponentTypes.PointsEnum)?.PointsAmount?.PointsCount > 0))) ?? false;
    }
}
