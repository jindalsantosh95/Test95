namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Models
{
    public class FindMyBookingResponse
    {
        public string BookingReference { get; set; }
        public DateOnly BookingDate { get; set; }       // The time is not relevant for the booking search, so we can use DateOnly here
        public IReadOnlyCollection<FindMyBookingPassenger> Passengers { get; set; }
        public IReadOnlyCollection<FindMyBookingFlight> Flights { get; set; }
        public IReadOnlyCollection<FindMyBookingTicket> Tickets { get; set; }
    }
}
