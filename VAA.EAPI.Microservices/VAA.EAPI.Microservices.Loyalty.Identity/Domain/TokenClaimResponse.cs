namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class TokenClaimResponse
    {
        public string FcNumber { get; set; }
        public string CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MemberType { get; set; }
        public string MemberStatus { get; set; }
        public string ExpTime { get; set; }
        public string Oid { get; set; }
    }
}