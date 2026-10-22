namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class SupplementarySilverAccountResponse
    {
        public SupplementarySilverMemberAccount SsaChildMember { get; set; }
        public SupplementarySilverMemberAccount SsaHeadMember { get; set; }

        public SupplementarySilverAccountResponse()
        {
        }
    }
}