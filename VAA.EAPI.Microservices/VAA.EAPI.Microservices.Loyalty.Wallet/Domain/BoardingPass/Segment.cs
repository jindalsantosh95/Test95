using NodaTime;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass
{
    public class Segment
    {
        public FlightCarrier OperatingCarrier { get; set; }
        public FlightCarrier MarketingCarrier { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public LocalDateTime? ScheduledArrivalLocalDateTime { get; set; }
        public AirportAndTerminal Origin { get; set; }
        public AirportAndTerminal Destination { get; set; }
        public int Ordinal { get; set; }
    }
}