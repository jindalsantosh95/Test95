namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Flights
    {
        public string? Ident { get; set; }
        public string? Airline { get; set; }
        public string? Duration { get; set; }
        public FiledDepartureTime? FiledDepartureTime { get; set; }
        public FiledArrivalTime? FiledArrivalTime { get; set; }
    }

}

