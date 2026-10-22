namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Slice
    {
        public string? Id { get; set; }
        public string? FareId { get; set; }
        public IEnumerable<string> FareFamilyTypes { get; set; }
        public IEnumerable<SliceFlightsAndFares> FlightsAndFares { get; set; }
    }
}
