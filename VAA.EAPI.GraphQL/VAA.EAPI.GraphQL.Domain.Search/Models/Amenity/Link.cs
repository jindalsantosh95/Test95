namespace VAA.EAPI.GraphQL.Domain.Search.Models.Amenity
{
    [GraphQLName("brandLink")]
    public class Link
    {
        public string Rel { get; set; }
        public string Href { get; set; }
    }
}
