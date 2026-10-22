namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities
{
    public class MemberTierUpdateEventData
    {
        public string InitiatedBy { get; set; }
        public string CustomerId { get; set; }
        public string OldTier { get; set; }
        public string NewTier { get; set; }
    }
}