namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Leg
    {
        public string Id { get; set; }
        public CarrierDetails Operating { get; set; }
        public CarrierDetails Marketing { get; set; }
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
        public string OnAirDuration { get; set; }
        public string Status { get; set; }
        public CabinClass CabinClass { get; set; }
        public Aircraft Aircraft { get; set; }
        public IEnumerable<SeatAssignment> SeatAssignments { get; set; }
        public string OnGroundDuration { get; set; }
        public string SeatSelectionType { get; set; }
    }
}
