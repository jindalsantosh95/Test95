namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Slice
    {
        public string Id { get; set; }
        public int DistanceMiles { get; set; }
        public int Ordinal { get; set; }
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
        public IEnumerable<Segment> Segments { get; set; }
        public string CheckinStatus { get; set; }
        public string TotalSliceDuration { get; set; }
        public bool CheckInEligible { get; set; }
        public IropProtect IropProtect { get; set; }
    }
}