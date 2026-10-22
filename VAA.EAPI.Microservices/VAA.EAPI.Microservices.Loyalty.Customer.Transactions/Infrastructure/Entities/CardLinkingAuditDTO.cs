using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class CardLinkingAuditDTO : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string CardLinkingUpdatedBy { get; set; }
        public string CardLinkingUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string CustomerId { get; set; }
        public string CardLinkingDocID { get; set; }
        public string PartnerCode { get; set; }
        public string StatusEnhancementCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public static explicit operator CardLinkingAuditDTO(PartnerMembership partnerMembership)
        {
            return new()
            {
                Id = partnerMembership.Id,
                PartitionKey = partnerMembership.CustomerId,
                CreatedBy = partnerMembership.CreatedBy,
                CardLinkingUpdatedBy = partnerMembership.LastUpdatedBy ?? partnerMembership.CreatedBy,
                SourceIdentifier = partnerMembership.SourceIdentifier ?? string.Empty,
                CustomerId = partnerMembership.CustomerId,
                CardLinkingDocID = partnerMembership.Id ?? string.Empty,
                PartnerCode = partnerMembership.PartnerCode ?? string.Empty,
                StatusEnhancementCode = partnerMembership.PartnerStatusCode ?? string.Empty,
                StartDate = partnerMembership.StartDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null,
                EndDate = partnerMembership.EndDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null
            };
        }
    }
}


