namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Models
{
    public class FindMyBookingTicket
    {
        public string PassengerRef { get; set; } = string.Empty;
        public DateOnly FlightDate { get; set; } = default;
        public int FlightNumber { get; set; } = default;
        public string TicketNumber { get; set; } = string.Empty;
        public string CouponNumber { get; set; } = string.Empty;

    }
}