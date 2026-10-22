namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class OutboundFlight
    {
        public string AirlineName { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public Departing Departing { get; set; }
        public Arriving Arriving { get; set; }
        public DateTime DepartingAt { get; set; }
        public DateTime ArrivingAt { get; set; }
        public string CabinClass { get; set; }
        public IEnumerable<Sector> Sectors { get; set; }
    }
}