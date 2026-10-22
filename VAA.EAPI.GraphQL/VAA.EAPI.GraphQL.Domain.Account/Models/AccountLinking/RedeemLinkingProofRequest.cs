namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    [HotChocolate.GraphQLName("LoyaltyRedeemLinkingProofInput")]
    public sealed class RedeemLinkingProofRequest
    {
        [HotChocolate.GraphQLName("proof"), HotChocolate.GraphQLNonNullType]
        [Newtonsoft.Json.JsonProperty("proof")]
        public string Proof { get; set; }
        [HotChocolate.GraphQLType(typeof(HotChocolate.Types.NonNullType<HotChocolate.Types.IdType>))]
        public string TransactionId { get; set; }
        [HotChocolate.GraphQLNonNullType]
        public string ClientId { get; set; }
        [HotChocolate.GraphQLNonNullType]
        public string Resource { get; set; }
    }
}
