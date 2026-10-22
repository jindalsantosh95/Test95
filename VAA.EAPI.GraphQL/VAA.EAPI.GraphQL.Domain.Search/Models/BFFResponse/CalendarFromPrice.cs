namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class CalendarFromPrice
    {
        public DateOnly FromDate { get; set; }
        public CalendarPrice Price { get; set; }

        public static explicit operator CalendarFromPrice(OfferMS.CalendarSearch.CalendarFromPrice source)
        {
            return new CalendarFromPrice
            {
                FromDate = source.FromDate,
                Price = (CalendarPrice)source.Price
            };
        }
    }
}
