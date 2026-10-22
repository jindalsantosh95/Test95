using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class FlyingClubMemberDTO : CosmosDocument
    {
        public List<MemberAttribute> MemberAttributes { get; set; }
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AutoAccrualStartDate { get; set; }
        public string GoldStartDate { get; set; }
        public string RetainSilvercounter { get; set; }
        public string CorporateMembershipNumber { get; set; }
        public string RetainGoldCounter { get; set; }
        public string LastAccrualTxnProcessDate { get; set; }
        public string LastRedemptionTxnProcessedDate { get; set; }
        public string LastTransactionProcessedDate { get; set; }
        public string LoungeNumber { get; set; }
        public string MemberNumber { get; set; }
        public string MemberType { get; set; }
        public string GroupId { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string TotalTierPoints { get; set; }
        public string VSOnlyTotalTierPoints { get; set; }
        public string VSFlownSectors { get; set; }
        public string ProgramId { get; set; }
        public string TierCode { get; set; }
        public string EnrollmentDate { get; set; }
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
        public string VAStatusChangeTimeStamp { get; set; }
        public string VRedSharedFlag { get; set; }
        public string EnrollmentChannel { get; set; }
        public MemberShipCardDetails MemberShipCardDetails { get; set; }

        public FlyingClubMemberDTO()
        {

        }

        public FlyingClubMemberDTO(FlyingClubMember fcmDoc)
        {
            SchemaVersion = Constants.Application.FlyingClubMemberDocSchemaVersion;
            Brand = Constants.Application.FlyingClubMemberBrand;
            DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType);

            Id = fcmDoc.Id;
            PartitionKey = fcmDoc.CustomerId;
            CustomerId = fcmDoc.CustomerId;
            CreatedOn = fcmDoc.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = fcmDoc.CreatedBy;
            LastUpdatedOn = fcmDoc.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = fcmDoc.LastUpdatedBy;
            SourceIdentifier = fcmDoc.SourceIdentifier ?? string.Empty;
            SourceSystem = fcmDoc.SourceSystem ?? string.Empty;
            MemberAttributes = new List<MemberAttribute>();
            foreach (Domain.FlyingClubMember.MemberAttribute memberAttribute in fcmDoc.MemberAttributes)
            {
                MemberAttribute memberAttribute1 = new()
                {
                    AttributeId = memberAttribute.AttributeId,
                    AttributeName = memberAttribute.AttributeName,
                    CreatedBy = memberAttribute.CreatedBy,
                    CreatedOn = memberAttribute.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = memberAttribute.LastUpdatedBy,
                    LastUpdatedOn = memberAttribute.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat),
                    SourceIdentifier = memberAttribute.SourceIdentifier,
                    SourceSystem = memberAttribute.SourceSystem,
                    Value = memberAttribute.Value,
                    MemberNumber = memberAttribute.MemberNumber
                };
                MemberAttributes.Add(memberAttribute1);
            }
            PartitionKey = fcmDoc.PartitionKey ?? string.Empty;
            AutoAccrualStartDate = fcmDoc.AutoAccrualStartDate ?? string.Empty;
            GoldStartDate = fcmDoc.GoldStartDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            RetainSilvercounter = fcmDoc.RetainSilvercounter.ToString();
            CorporateMembershipNumber = fcmDoc.CorporateMembershipNumber ?? string.Empty;
            RetainGoldCounter = fcmDoc.RetainGoldCounter.ToString();
            LastAccrualTxnProcessDate = fcmDoc.LastAccrualTxnProcessDate ?? string.Empty;
            LastRedemptionTxnProcessedDate = fcmDoc.LastRedemptionTxnProcessedDate ?? string.Empty;
            LastTransactionProcessedDate = fcmDoc.LastTransactionProcessedDate ?? string.Empty;
            LoungeNumber = fcmDoc.LoungeNumber ?? string.Empty;
            MemberNumber = fcmDoc.MemberNumber ?? string.Empty;
            MemberType = fcmDoc.MemberType ?? string.Empty;
            GroupId = fcmDoc.GroupId ?? string.Empty;
            HeadOfGroupMemberNumber = fcmDoc.HeadOfGroupMemberNumber ?? string.Empty;
            TotalTierPoints = fcmDoc.TotalTierPoints.ToString();
            VSOnlyTotalTierPoints = fcmDoc.VSOnlyTotalTierPoints.ToString();
            VSFlownSectors = fcmDoc.VSFlownSectors.ToString();
            ProgramId = fcmDoc.ProgramId ?? string.Empty;
            TierCode = fcmDoc.TierCode ?? string.Empty;
            EnrollmentDate = fcmDoc.EnrollmentDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            Status = fcmDoc.Status ?? string.Empty;
            MergeToMemberNumber = fcmDoc.MergeToMemberNumber ?? string.Empty;
            TestMemberFlag = fcmDoc.TestMemberFlag ?? string.Empty;
            TermsAndConditionsFlag = fcmDoc.TermsAndConditionsFlag ?? string.Empty;
            TelephonePassword = fcmDoc.TelephonePassword ?? string.Empty;
            MediaCode = fcmDoc.MediaCode ?? string.Empty;
            DowngradeProhibitFlag = fcmDoc.DowngradeProhibitFlag ?? string.Empty;
            SSAMemberNumber = fcmDoc.SSAMemberNumber ?? string.Empty;
            OnlineActivationFlag = fcmDoc.OnlineActivationFlag ?? string.Empty;
            OverrideGroupValidations = fcmDoc.OverrideGroupValidations ?? string.Empty;
            NameOnCard = fcmDoc.NameOnCard ?? string.Empty;
            CenturianFlag = fcmDoc.CenturianFlag ?? string.Empty;
            OverrideValidationFlag = fcmDoc.OverrideValidationFlag ?? string.Empty;
            OCDBFlag = fcmDoc.OCDBFlag ?? string.Empty;
            VAInstantGoldCampaignId = fcmDoc.VAInstantGoldCampaignId ?? string.Empty;
            LastTierChangeDate = fcmDoc.LastTierChangeDate ?? string.Empty;
            FastTrackId = fcmDoc.FastTrackId ?? string.Empty;
            InstantSilverCampaignId = fcmDoc.InstantSilverCampaignId ?? string.Empty;
            VAPartnerFileId = fcmDoc.VAPartnerFileId ?? string.Empty;
            VAStatusChangeTimeStamp = fcmDoc.VAStatusChangeTimeStamp?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            VRedSharedFlag = fcmDoc.VRedSharedFlag ?? string.Empty;
            EnrollmentChannel = fcmDoc.EnrollmentChannel;
            MemberShipCardDetails memberShipCardDetails = new()
            {
                Active = fcmDoc.MemberShipCardDetails?.Active,
                CardExpiryDate = fcmDoc.MemberShipCardDetails?.CardExpiryDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
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
                ExpiryDate = fcmDoc.MemberShipCardDetails?.ExpiryDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
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
        }
    }

    public class MemberAttribute
    {
        public string AttributeId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public string MemberNumber { get; set; }
    }
    public class MemberShipCardDetails
    {
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string Description { get; set; }
        public string ExpiryDate { get; set; }
        public string IssueDate { get; set; }
        public string MemberNumber { get; set; }
        public string NameOnCard { get; set; }
        public string RequestedDate { get; set; }
        public string StartDate { get; set; }
        public string CardStatusCode { get; set; }
        public string TierCode { get; set; }
        public string CardReplacementReason { get; set; }
        public string CardExpiryDate { get; set; }
        public string CardId { get; set; }
    }
}
