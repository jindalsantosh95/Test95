namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class FlightLeg
    {
        public int LegNumber { get; set; }
        public string OperatingCarrierCode { get; set; }
        public int OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public FlightTime ScheduleDepartureTime { get; set; }
        public FlightTime ScheduleArrivalTime { get; set; }
    }
}
