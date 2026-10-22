namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember
{
    public class TierRuleSetEntity
    {
        public string CustomerId { get; set; }
        public string S2GFlag { get; set; }
        public string S2SFTGFlag { get; set; }
        public string S2SFlag { get; set; }
        public string FirstVSFlightActivity { get; set; }
        public string FastTrackId { get; set; }
        public string VAInstantGoldId { get; set; }
        public string InstantSilverId { get; set; }
        public string CurrentStatus { get; set; }
        public string CurrentTier { get; set; }
        public string MemberType { get; set; }
        public string TierName { get; set; }
        public string MemberNumber { get; set; }
        public string ReferSSAGoldId { get; set; }
        public string SsaGoldTierName { get; set; }
        public string CenturionFlag { get; set; }
        public string DowngradeProhibitFlag { get; set; }
    }
}
