using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class LoyaltyGenericAudit : LoyaltyAudit
    {
        public string MemberNumber { get; set; }
        public string AuditCategory { get; set; }
        public string AuditSubCategory { get; set; }
        public string SourceIdentifier { get; set; }
        public string RelationshipType { get; set; }
        public string AuditData { get; set; }
        public string DataUpdatedBy { get; set; }
        public DateTime? DataUpdatedOn { get; set; }
    }
}