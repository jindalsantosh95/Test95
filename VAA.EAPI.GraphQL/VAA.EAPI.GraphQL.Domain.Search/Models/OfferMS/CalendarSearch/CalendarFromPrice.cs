namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch
{
    public class CalendarFromPrice
    {
        public DateOnly FromDate { get; set; }
        public CalendarPrice Price { get; set; }
    }
}