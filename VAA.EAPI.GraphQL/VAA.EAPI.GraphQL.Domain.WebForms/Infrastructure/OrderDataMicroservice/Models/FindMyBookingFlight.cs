namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Models
{
    public class FindMyBookingFlight
    {
        public DateOnly FlightDate { get; set; }
        public int FlightNumber { get; set; }
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
    }
}