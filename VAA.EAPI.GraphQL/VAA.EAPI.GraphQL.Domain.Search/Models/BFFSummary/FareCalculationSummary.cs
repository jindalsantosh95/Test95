namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class FareCalculationSummary
    {
        public IEnumerable<Fare>? Fares { get; set; }
        public IEnumerable<Taxes>? Taxes { get; set; }
        public string? TotalAwardPoints { get; set; }
    }
}
