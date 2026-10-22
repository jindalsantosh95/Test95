namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class OriginDestination
    {
        public string? OriginAirport { get; set; }
        public string? DestinationAirport { get; set; }
        public string? Distance { get; set; }
        public string? Duration { get; set; }
    }
}
