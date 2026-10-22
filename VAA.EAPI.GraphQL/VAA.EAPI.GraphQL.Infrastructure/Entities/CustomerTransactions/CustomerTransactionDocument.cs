using System;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{

    public class CustomerTransactionDocument
    {
        public string TierCode { get; set; }
        public int TierPoints { get; set; }
        public DateTime DateTierPointsReset { get; set; }
        public DateTime UpgradeByDate { get; set; }
        public int TierPointsNeedToReachNextTier { get; set; }
        public int FlownMiles { get; set; }
        public int RetainTierPoints { get; set; }
        public int TierPointsNeedToRetain { get; set; }
        public int RollingTierPoints { get; set; }
        public MembershipStatus MemberStatus { get; set; }
    }
}
