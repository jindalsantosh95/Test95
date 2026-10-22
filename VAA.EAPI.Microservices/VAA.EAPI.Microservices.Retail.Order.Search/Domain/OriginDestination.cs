namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }

        public string Distance { get; set; }
        public string Duration { get; set; }
        public string DepartureDate { get; set; }
    }
}
