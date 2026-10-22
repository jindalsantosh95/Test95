namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Segment
    {
        public string Id { get; set; }
        public int Ordinal { get; set; }
        public CarrierDetails OperatingCarrier { get; set; }
        public CarrierDetails MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime ScheduledArrivalUtcDateTime { get; set; }
        public DateTime ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public DateTime EstimatedDepartureLocalDateTime { get; set; }
        public DateTime EstimatedDepartureUtcDateTime { get; set; }
        public DateTime EstimatedArrivalLocalDateTime { get; set; }
        public DateTime EstimatedArrivalUtcDateTime { get; set; }
        public DateTime ActualDepartureLocalDateTime { get; set; }
        public DateTime ActualDepartureUtcDateTime { get; set; }
        public DateTime ActualArrivalLocalDateTime { get; set; }
        public DateTime ActualArrivalUtcDateTime { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public string ActionCode { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public string FlightSegmentId { get; set; }
        public AirlineDetails Operating { get; set; }
        public AirlineDetails Marketing { get; set; }
        public CabinClass BookedCabinClass { get; set; }
        public string CheckinStatus { get; set; }
        public string TotalSegmentDuration { get; set; }
        public string FareBasisCode { get; set; }
        public OperationalStatus OperationalStatus { get; set; }
        public bool CheckedIn { get; set; }
    }
}