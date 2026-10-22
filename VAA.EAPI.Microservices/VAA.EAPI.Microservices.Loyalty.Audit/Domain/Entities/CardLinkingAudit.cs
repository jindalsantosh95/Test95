using System;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities
{
    public class CardLinkingAudit : LoyaltyAudit
    {
        public string DocType { get; set; }
        public string CardLinkingUpdatedBy { get; set; }
        public DateTime? CardLinkingUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string CardLinkingDocID { get; set; }
        public string PartnerStatusCode { get; set; }
        public string StatusEnhancementCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerCode { get; set; }
    }
}












