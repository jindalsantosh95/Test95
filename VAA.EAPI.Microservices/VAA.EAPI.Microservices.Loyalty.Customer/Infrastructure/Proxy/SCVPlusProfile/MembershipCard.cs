using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class MembershipCard
    {
        public MembershipCard(string updatedBy = default, DateTime? updated = default, DateTime? startDate = default, DateTime? expiryDate = default, string tierName = default, string cardNumber = default)
        {
            UpdatedBy = updatedBy;
            Updated = updated;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            TierName = tierName;
            CardNumber = cardNumber;
        }

        public string UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string TierName { get; set; }
        public string CardNumber { get; set; }
    }
}
