namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
    }
}
