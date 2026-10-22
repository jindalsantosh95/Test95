using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class MembershipChangeDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

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

        public int Revision { get; set; }

        public string Comments { get; set; }

        public string ManualTierChangeReason { get; set; }

        public long QualifyingTierPoints { get; set; }

        public string Brand { get; set; }

        public string DowngradeProhibitReason { get; set; }
        public string ChangeId { get; set; }

        public MembershipChangeDTO(MembershipChange membershipChange)
        {
            if (membershipChange == null) throw new ArgumentNullException(nameof(membershipChange));


            Id = membershipChange.Id;
            PartitionKey = membershipChange.PartitionKey;
            DocType = membershipChange.DocType ?? string.Empty;
            CustomerId = membershipChange.CustomerId ?? string.Empty;
            SourceIdentifier = membershipChange.SourceIdentifier ?? string.Empty;
            SourceSystem = membershipChange.SourceSystem ?? string.Empty;
            CreatedOn = membershipChange.CreatedOn ?? string.Empty;
            CreatedBy = membershipChange.CreatedBy ?? string.Empty;
            LastUpdatedOn = membershipChange.LastUpdatedOn ?? string.Empty;
            LastUpdatedBy = membershipChange.LastUpdatedBy ?? string.Empty;
            Active = membershipChange.Active ?? string.Empty;
            AppliedRuleId = membershipChange.AppliedRuleId ?? string.Empty;
            AppliedPromotionId = membershipChange.AppliedPromotionId ?? string.Empty;
            ApprovalDate = membershipChange.ApprovalDate ?? string.Empty;
            TierApprovalStatus = membershipChange.TierApprovalStatus ?? string.Empty;
            EndDate = membershipChange.EndDate ?? string.Empty;
            MemberNumber = membershipChange.MemberNumber ?? string.Empty;
            OriginalTierEndDate = membershipChange.OriginalTierEndDate ?? string.Empty;
            TierPoints = membershipChange.TierPoints ?? string.Empty;
            ProcessDate = membershipChange.ProcessDate ?? string.Empty;
            ProcessingComment = membershipChange.ProcessingComment ?? string.Empty;
            ProcessingLog = membershipChange.ProcessingLog ?? string.Empty;
            ProcessingStatus = membershipChange.ProcessingStatus ?? string.Empty;
            QualifyingPeriodFlag = membershipChange.QualifyingPeriodFlag ?? string.Empty;
            StartDate = membershipChange.StartDate ?? string.Empty;
            TierCodeManuallyChangedBy = membershipChange.TierCodeManuallyChangedBy ?? string.Empty;
            TierClass = membershipChange.TierClass ?? string.Empty;
            TierCode = membershipChange.TierCode ?? string.Empty;
            Revision = membershipChange.Revision;
            Comments = membershipChange.Comments ?? string.Empty;
            ManualTierChangeReason = membershipChange.ManualTierChangeReason ?? string.Empty;
            QualifyingTierPoints = membershipChange.QualifyingTierPoints;
            Brand = membershipChange.Brand ?? string.Empty;
            DowngradeProhibitReason = membershipChange.DowngradeProhibitReason ?? string.Empty;
            ChangeId = membershipChange.ChangeId ?? string.Empty;


        }
    }
}
