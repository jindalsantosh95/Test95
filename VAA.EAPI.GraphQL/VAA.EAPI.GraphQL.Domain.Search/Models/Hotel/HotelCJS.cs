namespace VAA.EAPI.GraphQL.Domain.Search.Models.Hotel
{
    public class HotelCJS
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? BoardBasis { get; set; }
        public HotelCJSContent? Content { get; set; }
        public Tripadvisor? TripAdvisor { get; set; }
    }
}
