namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    [HotChocolate.GraphQLName("LoyaltyRedeemLinkingProofPayload")]
    public sealed class RedeemLinkingProofResponse
    {
        [HotChocolate.GraphQLNonNullType]
        public string FlyingClubAccountNumber { get; set; }
        [HotChocolate.GraphQLName("consentSubject"), HotChocolate.GraphQLNonNullType]
        [Newtonsoft.Json.JsonProperty("consentSubject")]
        public string ConsentSubject { get; set; }
    }
}
