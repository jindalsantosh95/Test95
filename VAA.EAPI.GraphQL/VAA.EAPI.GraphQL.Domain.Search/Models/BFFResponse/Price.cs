namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    [GraphQLName("PriceBFF")]
    public class Price
    {
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? AwardPoints { get; set; }

    }
}
