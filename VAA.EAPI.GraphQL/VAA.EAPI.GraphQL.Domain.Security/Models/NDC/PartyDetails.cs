namespace VAA.EAPI.GraphQL.Domain.Security.Models.NDC
{
    public class PartyDetails
    {
        [GraphQLDescription("Agency Details")]
        public NDCAgencyDetails? Agency { get; set; }

        [GraphQLDescription("Aggregator Details")]
        public NDCAggregatorDetails? Aggregator { get; set; }
    }
}
