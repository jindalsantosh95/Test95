using System;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities
{
    public class LoyaltyGenericAudit : LoyaltyAudit
    {
        public string SourceIdentifier { get; set; }
        public string AuditCategory { get; set; }
        public string AuditSubCategory { get; set; }
        public string RelationshipType { get; set; }
        public string AuditData { get; set; }
        public string DataUpdatedBy { get; set; }
        public DateTime? DataUpdatedOn { get; set; }
    }
}