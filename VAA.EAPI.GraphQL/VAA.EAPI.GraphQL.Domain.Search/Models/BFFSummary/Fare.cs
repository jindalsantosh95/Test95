namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Fare
    {
        public string? PassengerType { get; set; }
        public int Quantity { get; set; }
        public Price? Price { get; set; }
    }
}