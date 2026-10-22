using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities
{
    public class LoyaltyGenericAuditDTO : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string DataUpdatedBy { get; set; }
        public string DataUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string AuditCategory { get; set; }
        public string AuditSubCategory { get; set; }
        public string RelationshipType { get; set; }
        public string AuditData { get; set; }

        public static explicit operator LoyaltyGenericAuditDTO(LoyaltyGenericAudit loyaltyGenericAudit)
        {
            return new LoyaltyGenericAuditDTO()
            {
                DocType = DocumentType.OtherAudit.ToString(),
                Id = loyaltyGenericAudit?.Id,
                PartitionKey = loyaltyGenericAudit?.CustomerId,
                CustomerId = loyaltyGenericAudit?.CustomerId,
                CreatedOn = loyaltyGenericAudit?.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? null,
                CreatedBy = loyaltyGenericAudit?.CreatedBy ?? string.Empty,
                DataUpdatedOn = loyaltyGenericAudit?.DataUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? null,
                DataUpdatedBy = loyaltyGenericAudit?.DataUpdatedBy ?? string.Empty,
                SourceIdentifier = loyaltyGenericAudit?.SourceIdentifier ?? string.Empty,
                AuditCategory = loyaltyGenericAudit?.AuditCategory ?? string.Empty,
                AuditSubCategory = loyaltyGenericAudit?.AuditSubCategory ?? string.Empty,
                RelationshipType = loyaltyGenericAudit?.RelationshipType ?? string.Empty,
                AuditData = loyaltyGenericAudit?.AuditData ?? string.Empty
            };
        }
    }
}