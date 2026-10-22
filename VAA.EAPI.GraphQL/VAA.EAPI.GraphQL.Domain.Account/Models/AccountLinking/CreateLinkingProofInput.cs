namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    [HotChocolate.GraphQLName("AccountCreateLinkingProofInput")]
    public class CreateLinkingProofInput
    {
        [HotChocolate.GraphQLType(typeof(HotChocolate.Types.NonNullType<HotChocolate.Types.IdType>))]
        public string TransactionId { get; set; }
        [HotChocolate.GraphQLNonNullType]
        public string ClientId { get; set; }
        [HotChocolate.GraphQLNonNullType]
        public string Resource { get; set; }
        [HotChocolate.GraphQLNonNullType]
        public string Scope { get; set; }
    }
}
