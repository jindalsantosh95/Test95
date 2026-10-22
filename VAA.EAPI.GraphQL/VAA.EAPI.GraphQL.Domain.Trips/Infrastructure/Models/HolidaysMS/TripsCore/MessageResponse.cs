namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class MessageResponse
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<MessageDetail> Messages { get; set; }
    }
}
