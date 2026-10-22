namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class CalendarPrice
    {

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public bool MinimumPriceInWeek { get; set; }

        public bool MinimumPriceInMonth { get; set; }
        // Backwards compatibility - remove in #617045
        public int remaining { get => 0; }

        public bool Direct { get; set; }

        public int? AwardPoints { get; set; }

        public static explicit operator CalendarPrice(OfferMS.CalendarSearch.CalendarPrice source)
        {
            return new CalendarPrice
            {
                Amount = source.Amount,
                AwardPoints = source.AwardPoints,
                Currency = source.Currency,
                MinimumPriceInWeek = source.MinimumPriceInWeek,
                MinimumPriceInMonth = source.MinimumPriceInMonth,
                Direct = source.Direct
            };
        }
    }
}
