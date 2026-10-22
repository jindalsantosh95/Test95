namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    public class SeatSelectionRequest
    {
        public string TripReservationId { get; set; }
        public string SegmentId { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber { get; set; }
        public string SeatOfferId { get; set; }
    }
}
