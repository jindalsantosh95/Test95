using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class LoyaltyAudit
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CustomerId { get; set; }
    }
}