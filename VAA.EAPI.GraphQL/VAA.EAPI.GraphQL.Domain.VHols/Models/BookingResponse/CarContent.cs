namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class CarContent
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? CarClass { get; set; }
        public string? Transmission { get; set; }
        public int? Doors { get; set; }
        public bool? HasAirCon { get; set; }
        public bool? HasSatNav { get; set; }
        public Capacity? Capacity { get; set; }
        public Luggage? Luggage { get; set; }
    }
}

