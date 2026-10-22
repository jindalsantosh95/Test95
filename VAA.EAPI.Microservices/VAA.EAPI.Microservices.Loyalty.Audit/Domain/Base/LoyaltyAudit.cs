using System;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Base
{
    public abstract class LoyaltyAudit
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CustomerId { get; set; }
    }
}
