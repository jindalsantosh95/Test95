namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class Slice
    {
        public string Id { get; set; }
        public int DistanceMiles { get; set; }
        public int Ordinal { get; set; }
        public DateTime ScheduledDepartureLocal { get; set; }
        public DateTime ScheduledDepartureUtc { get; set; }
        public DateTime ScheduledArrivalLocal { get; set; }
        public DateTime ScheduledArrivalUtc { get; set; }
        public DateTime ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public DateTime EstimatedDepartureLocal { get; set; }
        public DateTime EstimatedDepartureUtc { get; set; }
        public DateTime EstimatedArrivalLocal { get; set; }
        public DateTime EstimatedArrivalUtc { get; set; }
        public DateTime ActualDepartureLocal { get; set; }
        public DateTime ActualDepartureUtc { get; set; }
        public DateTime ActualArrivalLocal { get; set; }
        public DateTime ActualArrivalUtc { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public string CheckinStatus { get; set; }
        public string TotalSliceDuration { get; set; }
        public bool CheckInEligible { get; set; }
        public IropProtect IropProtect { get; set; }
    }
}