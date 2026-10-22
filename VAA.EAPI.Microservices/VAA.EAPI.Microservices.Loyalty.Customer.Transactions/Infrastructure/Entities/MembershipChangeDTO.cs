using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class MembershipChangeDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string ChangeId { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string AppliedRuleId { get; set; }
        public string AppliedPromotionId { get; set; }
        public string ApprovalDate { get; set; }
        public string TierApprovalStatus { get; set; }
        public string EndDate { get; set; }
        public string MemberNumber { get; set; }
        public string OriginalTierEndDate { get; set; }
        public string TierPoints { get; set; }
        public string ProcessDate { get; set; }
        public string ProcessingComment { get; set; }
        public string ProcessingLog { get; set; }
        public string ProcessingStatus { get; set; }
        public string QualifyingPeriodFlag { get; set; }
        public string StartDate { get; set; }
        public string TierCodeManuallyChangedBy { get; set; }
        public string TierClass { get; set; }
        public string TierCode { get; set; }
        public string Comments { get; set; }
        public string ManualTierChangeReason { get; set; }
        public string QualifyingTierPoints { get; set; }
        public string DowngradeProhibitReason { get; set; }
        public string Revision { get; set; }

        public MembershipChangeDTO()
        {
            // Required for serialisation
        }

        public MembershipChangeDTO(MembershipChange tmDoc)
        {
            SchemaVersion = Constants.Application.TargetedMembershipDocSchemaVersion;
            Brand = Constants.Application.TargetedMembershipBrand;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.MembershipChangeDocType);
            ProcessDate = tmDoc.ProcessDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat);
            Id = tmDoc.Id;
            PartitionKey = tmDoc.CustomerId;
            ChangeId = tmDoc.ChangeId;
            CustomerId = tmDoc.CustomerId;
            CreatedOn = tmDoc.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = tmDoc.CreatedBy;
            LastUpdatedOn = tmDoc.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = tmDoc.LastUpdatedBy;
            Active = tmDoc.Active ?? string.Empty;
            SourceIdentifier = tmDoc.SourceIdentifier ?? string.Empty;
            SourceSystem = tmDoc.SourceSystem ?? string.Empty;
            AppliedRuleId = tmDoc.AppliedRuleId;
            AppliedPromotionId = tmDoc.AppliedPromotionId;
            ApprovalDate = tmDoc.ApprovalDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            TierApprovalStatus = tmDoc.TierApprovalStatus ?? string.Empty;
            EndDate = tmDoc.EndDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            MemberNumber = tmDoc.MemberNumber;
            OriginalTierEndDate = tmDoc.OriginalTierEndDate;
            TierPoints = tmDoc.TierPoints.ToString();
            ProcessDate = tmDoc.ProcessDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat);
            ProcessingComment = tmDoc.ProcessingComment;
            ProcessingLog = tmDoc.ProcessingLog;
            ProcessingStatus = tmDoc.ProcessingStatus;
            QualifyingPeriodFlag = tmDoc.QualifyingPeriodFlag;
            StartDate = tmDoc.StartDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            TierCodeManuallyChangedBy = tmDoc.TierCodeManuallyChangedBy ?? string.Empty;
            TierClass = tmDoc.TierClass;
            TierCode = tmDoc.TierCode ?? string.Empty;
            Comments = tmDoc.Comments ?? string.Empty;
            ManualTierChangeReason = tmDoc.ManualTierChangeReason ?? string.Empty;
            QualifyingTierPoints = string.IsNullOrEmpty(tmDoc.QualifyingTierPoints.ToString()) ? "0" : tmDoc.QualifyingTierPoints.ToString();
            DowngradeProhibitReason = tmDoc.DowngradeProhibitReason ?? string.Empty;
            Revision = tmDoc.Revision.ToString();
        }

        public MembershipChange ToMembershipChange()
        {
            return new MembershipChange(this);
        }
    }
}
