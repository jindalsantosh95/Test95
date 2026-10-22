namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Calendar
    {
        public IEnumerable<CalendarFromPrice> FromPrices { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}
