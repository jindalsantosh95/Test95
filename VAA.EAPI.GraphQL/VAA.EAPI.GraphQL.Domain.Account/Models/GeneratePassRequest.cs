namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class GeneratePassRequest
    {
        public string FcNumber { get; set; }
        public string MemberName { get; set; }
        public string TierCode { get; set; }
        public string ValidUntil { get; set; }
    }
}
