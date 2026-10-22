namespace VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking
{
    public sealed class CreateLinkingProofRequest : CreateLinkingProofInput
    {
        public string FlyingClubAccountNumber { get; set; }
    }
}
