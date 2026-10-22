namespace VAA.EAPI.GraphQL.Domain.Security.Models.NDC
{
    public class NDCAggregatorDetails
    {
        [GraphQLDescription("Aggregator Id")]
        public string AggregatorId { get; set; } = null!;

        [GraphQLDescription("Aggregator Name")]
        public string? AggregatorName { get; set; } = null!;

        [GraphQLDescription("Single Key Aggregator - Indicates if the aggregator subscription key is shared by all agents")]
        public bool? SingleKeyAggregator { get; set; } = false;
    }
}
