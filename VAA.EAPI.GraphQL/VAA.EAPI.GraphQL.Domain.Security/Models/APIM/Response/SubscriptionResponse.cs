using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response
{
    [GraphQLDescription("APIM Subscription Mutation response")]
    public class SubscriptionResponse
    {
        [GraphQLDescription("Subscription State")]
        public string SubscriptionState { get; set; } = null!;

        [GraphQLDescription("Subscription Keys")]
        public List<SubscriptionKey> SubscriptionKeys { get; set; } = null!;

        [GraphQLDescription("Channel")]
        public string Channel { get; set; } = null!;

        [GraphQLDescription("Environment")]
        public string Environment { get; set; } = null!;

        [GraphQLIgnore]
        public ErrorDetails Error { get; set; } = null!;

        public Status Status { get; set; }


    }
}
