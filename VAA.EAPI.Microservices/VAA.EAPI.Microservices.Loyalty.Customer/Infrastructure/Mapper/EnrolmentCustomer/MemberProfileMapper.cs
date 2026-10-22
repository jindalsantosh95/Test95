using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class MemberProfileMapper
    {
        public static MemberProfile MapEAPIMemberProfileToSCVMemberProfile(CreateCustomerEntities createCustomerEntities, string docType)
        {
            string UpdatedLastName = GetLastName.PopulateLastName(createCustomerEntities);
            return new MemberProfile()
            {
                TotalTierPoints = 0,
                FastTrackId = string.Empty,
                VsFlownSectors = 0,
                MemberNumber = createCustomerEntities?.FcNumber,
                GoldStartDate = string.Empty,
                NameOnCard = $"{createCustomerEntities?.FirstName} {UpdatedLastName}",
                VaInstantGoldCampaignId = string.Empty,
                Id = createCustomerEntities?.FcNumber,
                CustomerId = createCustomerEntities?.CustomerId,
                LastTierChangeDate = string.Empty,
                TierCode = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? TierCode.Glass.ToString() : createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount ? TierCode.VAC.ToString() : TierCode.Red.ToString(),
                RetainSilvercounter = 0,
                LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                Status = createCustomerEntities?.Status,
                CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                SourceIdentifier = string.Empty,
                CorporateMembershipNumber = string.Empty,
                ProgramId = string.Empty,
                MediaCode = createCustomerEntities?.MediaCode,
                Brand = Constants.Application.Brand,
                TermsAndConditionsFlag = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? "N" : createCustomerEntities?.TermsAndConditionsAccepted == true ? "Y" : "N",
                TestMemberFlag = createCustomerEntities?.TestMember == true ? "Y" : "N",
                PartitionKey = createCustomerEntities?.CustomerId,
                AutoAccrualStartDate = string.Empty,
                CenturianFlag = string.Empty,
                DowngradeProhibitFlag = string.Empty,
                LoungeNumber = string.Empty,
                OnlineActivationFlag = string.Empty,
                OverrideGroupValidations = string.Empty,
                InstantSilverCampaignId = string.Empty,
                OverrideValidationFlag = string.Empty,
                EnrollmentDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                MergeToMemberNumber = string.Empty,
                CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                DocType = docType,
                LastAccrualTxnProcessDate = string.Empty,
                VsOnlyTotalTierPoints = 0,
                LastTransactionProcessedDate = string.Empty,
                TelephonePassword = string.Empty,
                RetainGoldCounter = 0,
                MemberType = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? MemberTypeEnum.Individual.ToString() : createCustomerEntities?.MemberType?.ToString(),
                LastRedemptionTxnProcessedDate = string.Empty,
                SourceSystem = string.Empty,
                GroupID = string.Empty,
                OCDBFlag = string.Empty,
                MemberShipCardDetails = MembershipCardMapper.MapEAPIMembershipCardToSCVCard(createCustomerEntities),
                MemberAttributes = MembershipAttributeMapper.MapEAPIMembershipAttributeToSCV(createCustomerEntities),
                SSAMemberNumber = string.Empty,
                VAPartnerFileId = string.Empty,
                VAStatusChangeTimeStamp = string.Empty,
                HeadOfGroupMemberNumber = string.Empty,
                SchemaVersion = Constants.Application.SchemaVersion
            };
        }

    }
}
