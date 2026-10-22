namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
    }
}