namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.CalendarSearch
{
    public class CalendarPrice
    {
        public decimal Amount { get; set; }

        public int AwardPoints { get; set; }

        public string Currency { get; set; }
        public bool MinimumPriceInWeek { get; set; }

        public bool MinimumPriceInMonth { get; set; }

        public bool Direct { get; set; }
    }
}