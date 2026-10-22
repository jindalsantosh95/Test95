using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities
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

        public static explicit operator CardLinkingAuditDTO(CardLinkingAudit cardLinkingAudit)
        {
            return new()
            {

                DocType = ConfigReader.Get(Constants.Environment.CardLinkingAuditDocType),
                Id = cardLinkingAudit.Id,
                PartitionKey = cardLinkingAudit.CustomerId,
                CreatedBy = cardLinkingAudit.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                CardLinkingUpdatedBy = cardLinkingAudit.CardLinkingUpdatedBy ?? cardLinkingAudit.CreatedBy,
                CardLinkingUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                SourceIdentifier = cardLinkingAudit.SourceIdentifier ?? string.Empty,
                CustomerId = cardLinkingAudit.CustomerId,
                CardLinkingDocID = cardLinkingAudit.CardLinkingDocID ?? string.Empty,
                PartnerCode = cardLinkingAudit.PartnerCode ?? string.Empty,
                StatusEnhancementCode = cardLinkingAudit.StatusEnhancementCode ?? string.Empty,
                StartDate = cardLinkingAudit.StartDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null,
                EndDate = cardLinkingAudit.EndDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null
            };

        }
    }
}
