using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class HotelContent
    {
        public string? Name { get; set; }
        public string? Address { get; set; }

        [GraphQLName("Vrating")]
        public string? Vrating { get; set; }
        public List<string> Images { get; set; }
        public List<string> AvailableVariants { get; set; }
        public List<string> Variant { get; set; }
        public GeoCoordinates? GeoCoordinates { get; set; }
        public string? ParentLocation { get; set; }
        public string? Path { get; set; }
    }

}

