using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit
{
    public class LoyaltyGenericAudit
    {
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string AuditCategory { get; set; }
        public string AuditSubCategory { get; set; }
        public string RelationshipType { get; set; }
        public string AuditData { get; set; }
        public string DataUpdatedBy { get; set; }
        public DateTime? DataUpdatedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}