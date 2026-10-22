using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CardLinkingAudit : LoyaltyAudit
    {
        public string CardLinkingUpdatedBy { get; set; }
        public DateTime? CardLinkingUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string CardLinkingDocID { get; set; }
        public string PartnerCode { get; set; }
        public string StatusEnhancementCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
