namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.CampaignUnassign
{
    public class TargetedMembershipDeleteRequestData
    {
        public TargetedMembershipDeleteDocument TargetedMembershipDocument { get; set; }
        public string CustomerId { get; set; }
    }

    public class TargetedMembershipDeleteDocument
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
    }
}
