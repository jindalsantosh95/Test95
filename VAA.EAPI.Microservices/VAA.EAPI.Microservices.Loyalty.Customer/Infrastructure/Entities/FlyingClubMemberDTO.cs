using System;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class FlyingClubMemberDTO : CosmosDocument
    {
        public string AutoAccrualStartDate { get; set; }
        public string Brand { get; set; }
        public string CenturianFlag { get; set; }
        public string CorporateMembershipNumber { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string CustomerId { get; set; }
        public string DocType { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string EnrollmentDate { get; set; }
        public string FastTrackId { get; set; }
        public string GoldStartDate { get; set; }
        public string InstantSilverCampaignId { get; set; }
        public string LastAccrualTxnProcessDate { get; set; }
        public string LastRedemptionTxnProcessedDate { get; set; }
        public string LastTierChangeDate { get; set; }
        public string LastTransactionProcessedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LoungeNumber { get; set; }
        public string MediaCode { get; set; }
        public string MemberNumber { get; set; }
        public MemberShipCardDetails MemberShipCardDetails { get; set; }
        public string MemberType { get; set; }
        public string MergeToMemberNumber { get; set; }
        public string NameOnCard { get; set; }
        public string OnlineActivationFlag { get; set; }
        public string OverrideGroupValidations { get; set; }
        public string OverrideValidationFlag { get; set; }
        public string ProgramId { get; set; }
        public int? RetainGoldCounter { get; set; }
        public int? RetainSilvercounter { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Status { get; set; }
        public string TelephonePassword { get; set; }
        public string TermsAndConditionsFlag { get; set; }
        public string TestMemberFlag { get; set; }
        public string TierCode { get; set; }
        public int? TotalTierPoints { get; set; }
        public string VaInstantGoldCampaignId { get; set; }
        public int? VsFlownSectors { get; set; }
        public int? VsOnlyTotalTierPoints { get; set; }
        public List<MemberAttribute> MemberAttributes { get; set; }
        public string GroupID { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string SSAMemberNumber { get; set; }
        public string OCDBFlag { get; set; }
        public string VAPartnerFileId { get; set; }
        public string VAStatusChangeTimeStamp { get; set; }
        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

        public FlyingClubMemberDTO(FlyingClubMember fcmDoc)
        {
            if (fcmDoc == null) throw new ArgumentNullException(nameof(fcmDoc));

            Id = fcmDoc.Id;
            PartitionKey = fcmDoc.PartitionKey;
            AutoAccrualStartDate = fcmDoc.AutoAccrualStartDate ?? string.Empty;
            Brand = fcmDoc.Brand;
            CenturianFlag = fcmDoc.Brand ?? string.Empty;
            CorporateMembershipNumber = fcmDoc.CorporateMembershipNumber ?? string.Empty;
            CreatedBy = fcmDoc.CreatedBy ?? string.Empty;
            CreatedOn = fcmDoc.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            CustomerId = fcmDoc.CustomerId ?? string.Empty;
            DocType = fcmDoc.DocType ?? string.Empty;
            DowngradeProhibitFlag = fcmDoc.DowngradeProhibitFlag ?? string.Empty;
            EnrollmentDate = fcmDoc.EnrollmentDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            FastTrackId = fcmDoc.FastTrackId ?? string.Empty;
            GoldStartDate = fcmDoc.GoldStartDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            InstantSilverCampaignId = fcmDoc.InstantSilverCampaignId ?? string.Empty;
            LastAccrualTxnProcessDate = fcmDoc.LastAccrualTxnProcessDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            LastRedemptionTxnProcessedDate = fcmDoc.LastRedemptionTxnProcessedDate ?? string.Empty;
            LastTierChangeDate = fcmDoc.LastTierChangeDate ?? string.Empty;
            LastTransactionProcessedDate = fcmDoc.LastTransactionProcessedDate ?? string.Empty;
            LastUpdatedBy = fcmDoc.LastUpdatedBy ?? string.Empty;
            LastUpdatedOn = fcmDoc.LastUpdatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            LoungeNumber = fcmDoc.LoungeNumber ?? string.Empty;
            MediaCode = fcmDoc.MediaCode ?? string.Empty;
            MemberNumber = fcmDoc.MemberNumber ?? string.Empty;
            MemberShipCardDetails memberShipCardDetails = new()
            {
                Active = fcmDoc.MemberShipCardDetails?.Active,
                CardExpiryDate = fcmDoc.MemberShipCardDetails.CardExpiryDate != null ? fcmDoc.MemberShipCardDetails?.CardExpiryDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                CardId = fcmDoc.MemberShipCardDetails?.CardId,
                CardNumber = fcmDoc.MemberShipCardDetails?.CardNumber,
                CardReplacementReason = fcmDoc.MemberShipCardDetails?.CardReplacementReason,
                CardStatusCode = fcmDoc.MemberShipCardDetails?.CardStatusCode,
                CardType = fcmDoc.MemberShipCardDetails?.CardType,
                CreatedBy = fcmDoc.MemberShipCardDetails?.CreatedBy,
                CreatedOn = fcmDoc.MemberShipCardDetails?.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                NameOnCard = fcmDoc.MemberShipCardDetails?.NameOnCard,
                MemberNumber = fcmDoc.MemberShipCardDetails?.MemberNumber,
                Description = fcmDoc.MemberShipCardDetails?.Description,
                ExpiryDate = fcmDoc.MemberShipCardDetails.ExpiryDate != null ? fcmDoc.MemberShipCardDetails?.ExpiryDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                IssueDate = fcmDoc.MemberShipCardDetails?.IssueDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                SourceIdentifier = fcmDoc.MemberShipCardDetails?.SourceIdentifier,
                RequestedDate = fcmDoc.MemberShipCardDetails?.RequestedDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = fcmDoc.MemberShipCardDetails?.LastUpdatedBy,
                LastUpdatedOn = fcmDoc.MemberShipCardDetails?.LastUpdatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                SourceSystem = fcmDoc.MemberShipCardDetails?.SourceSystem,
                StartDate = fcmDoc.MemberShipCardDetails?.StartDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                TierCode = fcmDoc.MemberShipCardDetails?.TierCode
            };
            MemberShipCardDetails = memberShipCardDetails;
            MemberType = fcmDoc.MemberType ?? string.Empty;
            MergeToMemberNumber = fcmDoc.MergeToMemberNumber ?? string.Empty;
            NameOnCard = fcmDoc.NameOnCard ?? string.Empty;
            OnlineActivationFlag = fcmDoc.OnlineActivationFlag ?? string.Empty;
            OverrideGroupValidations = fcmDoc.OverrideGroupValidations ?? string.Empty;
            OverrideValidationFlag = fcmDoc.OverrideValidationFlag ?? string.Empty;
            ProgramId = fcmDoc.ProgramId ?? string.Empty;
            RetainGoldCounter = fcmDoc.RetainGoldCounter;
            RetainSilvercounter = fcmDoc.RetainSilvercounter;
            SourceIdentifier = fcmDoc.SourceIdentifier ?? string.Empty;
            SourceSystem = fcmDoc.SourceSystem ?? string.Empty;
            Status = fcmDoc.Status ?? string.Empty;
            TelephonePassword = fcmDoc.TelephonePassword ?? string.Empty;
            TermsAndConditionsFlag = fcmDoc.TermsAndConditionsFlag ?? string.Empty;
            TestMemberFlag = fcmDoc.TestMemberFlag ?? string.Empty;
            TierCode = fcmDoc.TierCode ?? string.Empty;
            VaInstantGoldCampaignId = fcmDoc.VaInstantGoldCampaignId ?? string.Empty;
            VsFlownSectors = fcmDoc.VsFlownSectors;
            MemberAttributes = new List<MemberAttribute>();
            foreach (Domain.Entities.MemberAttribute memberAttribute in fcmDoc.MemberAttributes)
            {
                MemberAttribute memberAttribute1 = new()
                {
                    AttributeName = memberAttribute.AttributeName,
                    CreatedBy = memberAttribute.CreatedBy,
                    CreatedOn = memberAttribute.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = memberAttribute.LastUpdatedBy,
                    LastUpdatedOn = memberAttribute.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat),
                    SourceIdentifier = memberAttribute.SourceIdentifier,
                    SourceSystem = memberAttribute.SourceSystem,
                    Value = memberAttribute.Value,
                    AttributeId = memberAttribute.AttributeId
                };
                MemberAttributes.Add(memberAttribute1);
            }
            HeadOfGroupMemberNumber = fcmDoc.HeadOfGroupMemberNumber;
            SSAMemberNumber = fcmDoc.SSAMemberNumber ?? string.Empty;
            OCDBFlag = fcmDoc.OCDBFlag ?? string.Empty;
            VAPartnerFileId = fcmDoc.VAPartnerFileId ?? string.Empty;
            VAStatusChangeTimeStamp = fcmDoc.VAStatusChangeTimeStamp ?? string.Empty;
            TotalTierPoints = fcmDoc.TotalTierPoints;
            GroupID = fcmDoc.GroupID;
            VsOnlyTotalTierPoints = fcmDoc.VsOnlyTotalTierPoints;
        }
    }
}
