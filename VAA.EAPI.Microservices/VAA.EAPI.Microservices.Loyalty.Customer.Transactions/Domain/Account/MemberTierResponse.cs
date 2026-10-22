using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Account
{
    public class MemberTierResponse : ICustomerTransactionQueryResponse
    {

        public string TierCode { get; set; }
        public int TierPoints { get; set; }
        public DateTime DateTierPointsReset { get; set; }
        public DateTime UpgradeByDate { get; set; }
        public int TierPointsNeedToReachNextTier { get; set; }
        public int TierPointsNeedToRetain { get; set; }
        public int FlownMiles { get; set; }
        public int RetainTierPoints { get; set; }
        public int RollingTierPoints { get; set; }
        public string CustomerId { get; set; }
        public MemberStatus MemberStatus { get; set; }


        public static explicit operator MemberTierResponse(MemberDetails source)
        {
            return new()
            {
                TierCode = source.TierCode,
                TierPoints = source.TierPoints,
                DateTierPointsReset = source.DateTierPointsReset,
                UpgradeByDate = source.UpgradeByDate,
                FlownMiles = source.FlownMiles,
                TierPointsNeedToReachNextTier = source.TierPointsNeedToReachNextTier,
                TierPointsNeedToRetain = source.TierPointsNeedToRetain,
                RetainTierPoints = source.RetainTierPoints,
                RollingTierPoints = source.RollingTierPoints,
                MemberStatus = source.MemberStatus
            };
        }
    }
}
