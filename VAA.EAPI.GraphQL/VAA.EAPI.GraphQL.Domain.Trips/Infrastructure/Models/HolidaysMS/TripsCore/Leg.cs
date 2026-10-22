namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class Leg
    {
        public string Id { get; set; }
        public CarrierDetails Operating { get; set; }
        public CarrierDetails Marketing { get; set; }
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }
        public DateTime? ScheduledDepartureLocal { get; set; }
        public DateTime? ScheduledArrivalLocal { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public DateTime? EstimatedDepartureLocal { get; set; }
        public DateTime? EstimatedArrivalLocal { get; set; }
        public DateTime? ActualDepartureLocal { get; set; }
        public DateTime? ActualArrivalLocal { get; set; }
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