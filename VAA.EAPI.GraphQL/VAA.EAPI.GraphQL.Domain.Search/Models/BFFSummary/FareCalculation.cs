namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class FareCalculation
    {
        public string? PassengerType { get; set; }
        public Price? Fare { get; set; }
        public IEnumerable<Taxes>? Taxes { get; set; }
    }
}