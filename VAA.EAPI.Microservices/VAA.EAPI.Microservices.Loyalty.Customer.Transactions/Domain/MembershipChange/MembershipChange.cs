using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange
{
    public class MembershipChange : CustomerTransaction
    {
        public string ChangeId { get; set; }
        public string Active { get; set; }
        public string AppliedRuleId { get; set; }
        public string AppliedPromotionId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string TierApprovalStatus { get; set; }
        public DateTime? EndDate { get; set; }
        public string MemberNumber { get; set; }
        public string OriginalTierEndDate { get; set; }
        public int? TierPoints { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string ProcessingComment { get; set; }
        public string ProcessingLog { get; set; }
        public string ProcessingStatus { get; set; }
        public string QualifyingPeriodFlag { get; set; }
        public DateTime? StartDate { get; set; }
        public string TierCodeManuallyChangedBy { get; set; }
        public string TierClass { get; set; }
        public string TierCode { get; set; }
        public string Comments { get; set; }
        public string ManualTierChangeReason { get; set; }
        public int? QualifyingTierPoints { get; set; }
        public string DowngradeProhibitReason { get; set; }
        public int? Revision { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public MembershipChange()
        {
            // Required for Serialization
        }

        public MembershipChange(MembershipChangeDTO dto)
        {
            SourceIdentifier = dto.SourceIdentifier;
            SourceSystem = dto.SourceSystem;
            ProcessDate = !string.IsNullOrEmpty(dto.ProcessDate) ? DateTime.Parse(dto.ProcessDate) : null;
            StartDate = !string.IsNullOrEmpty(dto.StartDate) ? DateTime.Parse(dto.StartDate) : null;
            EndDate = !string.IsNullOrEmpty(dto.EndDate) ? DateTime.Parse(dto.EndDate) : null;
            ChangeId = dto.ChangeId;
            AppliedRuleId = dto.AppliedRuleId;
            AppliedPromotionId = dto.AppliedPromotionId;
            TierApprovalStatus = dto.TierApprovalStatus;
            OriginalTierEndDate = dto.OriginalTierEndDate;
            TierPoints = string.IsNullOrEmpty(dto.TierPoints) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.TierPoints));
            ProcessingComment = dto.ProcessingComment;
            ProcessingLog = dto.ProcessingLog;
            ProcessingStatus = dto.ProcessingStatus;
            QualifyingPeriodFlag = dto.QualifyingPeriodFlag;
            TierCodeManuallyChangedBy = dto.TierCodeManuallyChangedBy;
            TierClass = dto.TierClass;
            TierCode = dto.TierCode;
            Comments = dto.Comments;
            ManualTierChangeReason = dto.ManualTierChangeReason;
            QualifyingTierPoints = string.IsNullOrEmpty(dto.QualifyingTierPoints) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.QualifyingTierPoints));
            DowngradeProhibitReason = dto.DowngradeProhibitReason;
            Revision = string.IsNullOrEmpty(dto.Revision) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.Revision));
            Id = dto.Id;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(dto.LastUpdatedOn) ? DateTime.Parse(dto.LastUpdatedOn) : DateTime.Now.ToUniversalTime();
            CustomerId = dto.CustomerId;
            Active = dto.Active;
            MemberNumber = dto.MemberNumber;
        }
    }
}
