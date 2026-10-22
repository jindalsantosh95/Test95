namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch
{
    public class DepartureDetails
    {
        public DateTime? DepartureDate { get; set; }
        public IEnumerable<FlexibleDatePriceCalendar> FlexibleDatePriceCalendarCell { get; set; }
    }
}
