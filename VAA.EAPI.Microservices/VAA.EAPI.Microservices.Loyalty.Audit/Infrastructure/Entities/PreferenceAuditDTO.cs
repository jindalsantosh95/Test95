using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities
{
    public class PreferenceAuditDTO : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string PreferenceUpdatedBy { get; set; }
        public string PreferenceUpdatedOn { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string VAMarketingPreference { get; set; }
        public string VAPartnersMarketingPermissions { get; set; }
        public string DoNotCall { get; set; }
        public string DoNotEmail { get; set; }
        public string DoNotSMS { get; set; }
        public string PreferredPartnerFFP { get; set; }
        public string Language { get; set; }

        public PreferenceAuditDTO(PreferenceAudit auditDoc)
        {
            DocType = ConfigReader.Get(Constants.Environment.PreferenceAuditDocType);
            Id = auditDoc?.Id;
            PartitionKey = auditDoc?.CustomerId;
            CustomerId = auditDoc?.CustomerId;
            CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            CreatedBy = auditDoc?.PreferenceUpdatedBy ?? string.Empty;
            PreferenceUpdatedOn = auditDoc?.PreferenceUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat) ?? null;
            PreferenceUpdatedBy = auditDoc?.PreferenceUpdatedBy ?? string.Empty;
            SourceIdentifier = auditDoc?.SourceIdentifier ?? string.Empty;
            VAMarketingPreference = auditDoc?.VAMarketingPreference ?? string.Empty;
            DoNotCall = auditDoc?.DoNotCall ?? string.Empty;
            DoNotEmail = auditDoc?.DoNotEmail ?? string.Empty;
            DoNotSMS = auditDoc?.DoNotSMS ?? string.Empty;
            PreferredPartnerFFP = auditDoc?.PreferredPartnerFFP ?? string.Empty;
            Language = auditDoc?.Language ?? string.Empty;

        }
        public string EmailCommunication { get; set; }
        public string PostCommunication { get; set; }
        public string FaxCommunication { get; set; }
        public string FlyingClubNews { get; set; }
        public string PhoneCommunication { get; set; }
        public string DoNotPost { get; set; }
        public string DoNotFax { get; set; }
        public string Communication { get; set; }
        public string DoNotMail { get; set; }
    }
}