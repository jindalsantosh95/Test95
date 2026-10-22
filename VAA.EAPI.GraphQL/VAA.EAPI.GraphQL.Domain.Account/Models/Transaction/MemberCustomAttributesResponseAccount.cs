namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class MemberCustomAttributesResponseAccount
    {
        public IEnumerable<MemberCustomAttributesAccount> MemberCustomAttributesDocuments { get; set; }
        public ErrorResponse ErrorResponse { get; set; }

    }
}
