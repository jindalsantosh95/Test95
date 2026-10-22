namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    [GraphQLName("BrandLeg")]
    public class Leg
    {
        public TimeSpan? TravelTime { get; set; }
        public ICollection<Fare>? Fares { get; set; }
    }
}