namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class HolidaySearchRequest
    {
        public string? BookingType { get; set; }
        public string? Location { get; set; }
        public string? Gateway { get; set; }
        public string? DepartureDate { get; set; }
        public string? Duration { get; set; }
        public IEnumerable<PartyComposition> PartyCompositions { get; set; } = Enumerable.Empty<PartyComposition>();
    }
}
