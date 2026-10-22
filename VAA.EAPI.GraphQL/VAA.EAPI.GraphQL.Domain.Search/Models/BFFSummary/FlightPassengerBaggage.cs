namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class FlightPassengerBaggage
    {
        public string? FlightRefId { get; set; }
        public CarryOnBag? CarryOnBag { get; set; }
        public CheckedBag? CheckedBag { get; set; }
    }
}
