namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class SeatAssignment
    {
        public Seat Seat { get; set; }

        public static explicit operator SeatAssignment(Infrastructure.Proxy.DeltaMyTrips.SeatAssignment source)
        {
            return new SeatAssignment
            {
                Seat = (Seat)source.Seat
            };
        }
    }
}
