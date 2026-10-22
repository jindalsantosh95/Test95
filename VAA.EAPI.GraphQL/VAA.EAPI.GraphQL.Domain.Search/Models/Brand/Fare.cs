namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    [GraphQLName("Fare_Brand")]
    public class Fare
    {
        public string? Product { get; set; }
        public ICollection<Segment>? Segments { get; set; }
        public Fees? Fees { get; set; } = new Fees();
    }
}