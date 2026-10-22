using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember
{
    public class FlyingClubMember : CustomerTransaction
    {
        public List<MemberAttribute> MemberAttributes { get; set; }
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string PartitionKey { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string AutoAccrualStartDate { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public int? RetainSilvercounter { get; set; }
        public string CorporateMembershipNumber { get; set; }
        public int? RetainGoldCounter { get; set; }
        public string LastAccrualTxnProcessDate { get; set; }
        public string LastRedemptionTxnProcessedDate { get; set; }
        public string LastTransactionProcessedDate { get; set; }
        public string LoungeNumber { get; set; }
        public string MemberNumber { get; set; }
        public string MemberType { get; set; }
        public string GroupId { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public int? TotalTierPoints { get; set; }
        public int? VSOnlyTotalTierPoints { get; set; }
        public int? VSFlownSectors { get; set; }
        public string ProgramId { get; set; }
        public string TierCode { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string Status { get; set; }
        public string MergeToMemberNumber { get; set; }
        public string TestMemberFlag { get; set; }
        public string TermsAndConditionsFlag { get; set; }
        public string TelephonePassword { get; set; }
        public string MediaCode { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string SSAMemberNumber { get; set; }
        public string OnlineActivationFlag { get; set; }
        public string OverrideGroupValidations { get; set; }
        public string NameOnCard { get; set; }
        public string CenturianFlag { get; set; }
        public string OverrideValidationFlag { get; set; }
        public string OCDBFlag { get; set; }
        public string VAInstantGoldCampaignId { get; set; }
        public string LastTierChangeDate { get; set; }
        public string FastTrackId { get; set; }
        public string InstantSilverCampaignId { get; set; }
        public string VAPartnerFileId { get; set; }
        public DateTime? VAStatusChangeTimeStamp { get; set; }
        public string VRedSharedFlag { get; set; }
        public string EnrollmentChannel { get; set; }
        public MemberShipCardDetails MemberShipCardDetails { get; set; }
        public FlyingClubMember()
        {
            // Required for Serialization
        }

        public FlyingClubMember(FlyingClubMemberDTO dto)
        {
            Id = dto.Id;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(dto.LastUpdatedOn) ? DateTime.Parse(dto.LastUpdatedOn) : DateTime.Now.ToUniversalTime();
            CustomerId = dto.CustomerId;
            SourceIdentifier = dto.SourceIdentifier;
            SourceSystem = dto.SourceSystem;
            MemberAttributes = new List<MemberAttribute>();
            foreach (Infrastructure.Entities.MemberAttribute memberAttribute in dto.MemberAttributes)
            {
                MemberAttribute memberAttribute1 = new MemberAttribute
                {
                    AttributeId = memberAttribute.AttributeId,
                    AttributeName = memberAttribute.AttributeName,
                    CreatedBy = memberAttribute.CreatedBy,
                    CreatedOn = string.IsNullOrEmpty(memberAttribute?.CreatedOn) ? null : DateTime.Parse(memberAttribute?.CreatedOn),
                    LastUpdatedBy = memberAttribute.LastUpdatedBy,
                    LastUpdatedOn = string.IsNullOrEmpty(memberAttribute?.LastUpdatedOn) ? null : DateTime.Parse(memberAttribute?.LastUpdatedOn),
                    SourceIdentifier = memberAttribute.SourceIdentifier,
                    SourceSystem = memberAttribute.SourceSystem,
                    Value = memberAttribute.Value,
                    MemberNumber = memberAttribute.MemberNumber
                };
                MemberAttributes.Add(memberAttribute1);
            }
            PartitionKey = dto.PartitionKey;
            AutoAccrualStartDate = dto.AutoAccrualStartDate;
            GoldStartDate = !string.IsNullOrEmpty(dto.GoldStartDate) ? DateTime.Parse(dto.GoldStartDate) : null;
            RetainSilvercounter = string.IsNullOrEmpty(dto.RetainSilvercounter) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.RetainSilvercounter));
            CorporateMembershipNumber = dto.CorporateMembershipNumber;
            RetainGoldCounter = string.IsNullOrEmpty(dto.RetainGoldCounter) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.RetainGoldCounter));
            LastAccrualTxnProcessDate = dto.LastAccrualTxnProcessDate;
            LastRedemptionTxnProcessedDate = dto.LastRedemptionTxnProcessedDate;
            LastTransactionProcessedDate = dto.LastTransactionProcessedDate;
            LoungeNumber = dto.LoungeNumber;
            MemberNumber = dto.MemberNumber;
            MemberType = dto.MemberType;
            GroupId = dto.GroupId;
            HeadOfGroupMemberNumber = dto.HeadOfGroupMemberNumber;
            TotalTierPoints = string.IsNullOrEmpty(dto.TotalTierPoints) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.TotalTierPoints));
            VSOnlyTotalTierPoints = string.IsNullOrEmpty(dto.VSOnlyTotalTierPoints) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.VSOnlyTotalTierPoints));
            VSFlownSectors = string.IsNullOrEmpty(dto.VSFlownSectors) ? 0 : Convert.ToInt32(Convert.ToDouble(dto.VSFlownSectors));
            ProgramId = dto.ProgramId;
            TierCode = dto.TierCode;
            EnrollmentDate = !string.IsNullOrEmpty(dto.EnrollmentDate) ? DateTime.Parse(dto.EnrollmentDate) : null;
            Status = dto.Status;
            MergeToMemberNumber = dto.MergeToMemberNumber;
            TestMemberFlag = dto.TestMemberFlag;
            TermsAndConditionsFlag = dto.TermsAndConditionsFlag;
            TelephonePassword = dto.TelephonePassword;
            MediaCode = dto.MediaCode;
            DowngradeProhibitFlag = dto.DowngradeProhibitFlag;
            SSAMemberNumber = dto.SSAMemberNumber;
            OnlineActivationFlag = dto.OnlineActivationFlag;
            OverrideGroupValidations = dto.OverrideGroupValidations;
            NameOnCard = dto.NameOnCard;
            CenturianFlag = dto.CenturianFlag;
            OverrideValidationFlag = dto.OverrideValidationFlag;
            OCDBFlag = dto.OCDBFlag;
            VAInstantGoldCampaignId = dto.VAInstantGoldCampaignId;
            LastTierChangeDate = dto.LastTierChangeDate;
            FastTrackId = dto.FastTrackId;
            InstantSilverCampaignId = dto.InstantSilverCampaignId;
            VAPartnerFileId = dto.VAPartnerFileId;
            VAStatusChangeTimeStamp = !string.IsNullOrEmpty(dto.VAStatusChangeTimeStamp) ? DateTime.Parse(dto.VAStatusChangeTimeStamp) : null;
            VRedSharedFlag = dto.VRedSharedFlag;
            EnrollmentChannel = dto.EnrollmentChannel;
            MemberShipCardDetails = new MemberShipCardDetails
            {
                Active = dto.MemberShipCardDetails.Active,
                CardExpiryDate = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.CardExpiryDate) ? DateTime.Parse(dto.MemberShipCardDetails?.CardExpiryDate) : null,
                CardId = dto.MemberShipCardDetails.CardId,
                CardNumber = dto.MemberShipCardDetails.CardNumber,
                CardReplacementReason = dto.MemberShipCardDetails.CardReplacementReason,
                CardStatusCode = dto.MemberShipCardDetails.CardStatusCode,
                CardType = dto.MemberShipCardDetails.CardType,
                CreatedBy = dto.MemberShipCardDetails.CreatedBy,
                CreatedOn = !string.IsNullOrEmpty(dto.MemberShipCardDetails.CreatedOn) ? DateTime.Parse(dto.MemberShipCardDetails.CreatedOn) : null,
                NameOnCard = dto.MemberShipCardDetails.NameOnCard,
                MemberNumber = dto.MemberShipCardDetails.MemberNumber,
                Description = dto.MemberShipCardDetails.Description,
                ExpiryDate = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.ExpiryDate) ? DateTime.Parse(dto.MemberShipCardDetails?.ExpiryDate) : null,
                IssueDate = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.IssueDate) ? DateTime.Parse(dto.MemberShipCardDetails?.IssueDate) : null,
                SourceIdentifier = dto.MemberShipCardDetails.SourceIdentifier,
                RequestedDate = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.RequestedDate) ? DateTime.Parse(dto.MemberShipCardDetails?.RequestedDate) : null,
                LastUpdatedBy = dto.MemberShipCardDetails.LastUpdatedBy,
                LastUpdatedOn = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.LastUpdatedOn) ? DateTime.Parse(dto.MemberShipCardDetails?.LastUpdatedOn) : null,
                SourceSystem = dto.MemberShipCardDetails.SourceSystem,
                StartDate = !string.IsNullOrEmpty(dto.MemberShipCardDetails?.StartDate) ? DateTime.Parse(dto.MemberShipCardDetails?.StartDate) : null,
                TierCode = dto.MemberShipCardDetails.TierCode
            };
        }
    }
    public class MemberAttribute
    {
        public string AttributeId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public string MemberNumber { get; set; }
        public string SSAMemberNumber { get; set; }

    }
    public class MemberShipCardDetails
    {
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string MemberNumber { get; set; }
        public string NameOnCard { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CardStatusCode { get; set; }
        public string TierCode { get; set; }
        public string CardReplacementReason { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public string CardId { get; set; }
    }
}
