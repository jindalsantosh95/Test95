namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign
{
    public class TargetedMembershipDocDeleteEventData
    {
        public TargetedMembershipDeleteDocument TargetedMembershipDocument { get; set; }
        public string CustomerId { get; set; }
    }
}
