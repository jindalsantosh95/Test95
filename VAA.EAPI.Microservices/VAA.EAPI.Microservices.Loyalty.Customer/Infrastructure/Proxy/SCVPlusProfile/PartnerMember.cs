using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class PartnerMember
    {
        public PartnerMember(string partnerCode = default, DateTime? startDate = default, DateTime? endDate = default, string statusEnhancementCode = default)
        {
            PartnerCode = partnerCode;
            StartDate = startDate;
            EndDate = endDate;
            StatusEnhancementCode = statusEnhancementCode;
        }

        public string PartnerCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StatusEnhancementCode { get; set; }
    }
}
