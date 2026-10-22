using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities
{
    public class AwardAuditDTO : CosmosDocument
    {
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string AwardUpdatedBy { get; set; }
        public string AwardUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string AwardNumber { get; set; }
        public string AwardName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public string UsedDate { get; set; }
        public string AwardId { get; set; }
        public string AwardProductId { get; set; }
        public string GiftedAwardDate { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }
        public string DocType { get; set; }
        public string GiftedFromMemberNumber { get; set; }
        public string GiftedToMemberNumber { get; set; }

        public AwardAuditDTO()
        {
            // Required for serialisation
        }

        public AwardAuditDTO(AwardAudit award)
        {
            Id = award.Id;
            DocType = ConfigReader.Get(Constants.Environment.AwardAuditDocType);
            PartitionKey = award.CustomerId;
            CustomerId = award.CustomerId;
            CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = award.AwardUpdatedBy ?? string.Empty;
            AwardUpdatedOn = award.AwardUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            AwardUpdatedBy = award.AwardUpdatedBy ?? string.Empty;
            AwardId = award.AwardId ?? string.Empty;
            AwardNumber = award.AwardNumber ?? string.Empty;
            AwardName = award.AwardName ?? string.Empty;
            AwardProductId = award.AwardProductId ?? string.Empty;
            StartDate = award.StartDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            EndDate = award.EndDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            Status = award.Status ?? string.Empty;
            UsedDate = award.UsedDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            GiftedAwardDate = award.GiftedAwardDate?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            GiftedFromCustomerId = award.GiftedFromCustomerId ?? string.Empty;
            GiftedToCustomerId = award.GiftedToCustomerId ?? string.Empty;
            PreviousStatus = award.PreviousStatus ?? string.Empty;
            SourceIdentifier = string.Empty;
            GiftedFromMemberNumber = award.GiftedFromMemberNumber ?? string.Empty;
            GiftedToMemberNumber = award.GiftedToMemberNumber ?? string.Empty;
        }
    }
}