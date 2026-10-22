namespace VAA.EAPI.GraphQL.Domain.Operations.DTOs.PassengerManifest_SCVv7
{
    public class Connection
    {
        public string ConnectionType { get; set; }
        public string RecordLocator { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string FlightOrigin { get; set; }
        public string FlightDestination { get; set; }
        public DateTime FlightDepartureDateTime { get; set; }
        public DateTime FlightArrivalDateTime { get; set; }
        public string ConnectionTime { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingCarrierCode { get; set; }
    }
}
