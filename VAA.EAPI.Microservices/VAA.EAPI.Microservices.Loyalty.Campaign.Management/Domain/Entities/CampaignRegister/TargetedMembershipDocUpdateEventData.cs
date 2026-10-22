namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister
{
    public class TargetedMembershipDocUpdateEventData
    {
        public TargetedMembershipUpdateDocument TargetedMembershipDocument { get; set; }
        public string CustomerId { get; set; }
    }
}
