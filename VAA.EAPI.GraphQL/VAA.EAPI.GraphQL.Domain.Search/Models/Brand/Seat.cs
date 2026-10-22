namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    [GraphQLName("BrandSeat")]
    public class Seat : AmenityBase
    {
        public string? Flatness { get; set; }
        public string? Legroom { get; set; }
        public string? Pitch { get; set; }
        public string? Width { get; set; }
    }
}