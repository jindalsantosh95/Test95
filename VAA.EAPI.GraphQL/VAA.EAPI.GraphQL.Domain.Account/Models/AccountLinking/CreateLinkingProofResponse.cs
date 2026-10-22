namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    [HotChocolate.GraphQLName("AccountCreateLinkingProofPayload")]
    public sealed class CreateLinkingProofResponse
    {
        [HotChocolate.GraphQLName("proof"), HotChocolate.GraphQLNonNullType]
        [Newtonsoft.Json.JsonProperty("proof")]
        public string Proof { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
