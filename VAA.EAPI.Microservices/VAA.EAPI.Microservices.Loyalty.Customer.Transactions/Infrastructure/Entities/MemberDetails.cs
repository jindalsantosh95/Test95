using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Account;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class MemberDetails
    {
        public string TierCode { get; set; }
        public int TierPoints { get; set; }
        public DateTime UpgradeByDate { get; set; }
        public DateTime DateTierPointsReset { get; set; }
        public int TierPointsNeedToReachNextTier { get; set; }
        public int TierPointsNeedToRetain { get; set; }
        public int RollingTierPoints { get; set; }
        public int FlownMiles { get; set; }
        public int RetainTierPoints { get; set; }
        public MemberStatus MemberStatus { get; set; }
    }
}
