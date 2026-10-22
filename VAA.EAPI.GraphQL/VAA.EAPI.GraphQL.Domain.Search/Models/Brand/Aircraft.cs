namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    [GraphQLName("BrandAircraft")]
    public class Aircraft : AmenityBase
    {
        public string? Cabin_Pressure { get; set; }
        public string? Model { get; set; }
        public string? Window_Size { get; set; }
    }
}