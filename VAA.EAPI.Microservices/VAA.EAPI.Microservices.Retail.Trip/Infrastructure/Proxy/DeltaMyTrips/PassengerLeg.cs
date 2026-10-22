namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class PassengerLeg
    {
        public int LegId { get; set; }
        public IEnumerable<SeatAssignment>? SeatAssignments { get; set; }
    }


}
