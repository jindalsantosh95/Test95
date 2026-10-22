using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Vred.Constants;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredConsent;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper
{
    public static class VRedRequestMapper
    {
        internal static MemberEnrolmentVRedRequest MapToVRedRequest(CreateCustomerEntities createCustomerEntities)
        {
            if (createCustomerEntities == null) throw new ArgumentNullException(nameof(createCustomerEntities));

            PartnerMetadata partnerMetaData = new PartnerMetadata
            {
                Address = new Address
                {
                    CountryCode = createCustomerEntities.AddressDetails?.FirstOrDefault()?.CountryCode
                },

                Gender = createCustomerEntities.Gender,
                JoinDatetime = Common.Helpers.DateTimeHelper.ConvertToUtc(createCustomerEntities.ApprovalDate, Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat),
                TierInfo = new TierInfo
                {
                    CurrentTier = createCustomerEntities.CurrentTier,
                    PreviousTier = null,
                    TierChangeDatetime = Common.Helpers.DateTimeHelper.ConvertToUtc(createCustomerEntities.ApprovalDate, Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat),
                },
                MemberType = createCustomerEntities.MemberType,
                Schema = ConfigReader.Get(Constants.Environment.MemberMetaDataSchema)
            };

            string partnerMetadataContent = Json.Serialize(partnerMetaData, new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.None
            });

            return new MemberEnrolmentVRedRequest
            {
                PartnerUserId = createCustomerEntities.FcNumber,
                FirstName = createCustomerEntities.FirstName,
                LastName = createCustomerEntities.LastName,
                Email = createCustomerEntities.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId,
                Mobile = createCustomerEntities.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PhoneNumber,
                Country = createCustomerEntities.AddressDetails?.FirstOrDefault()?.CountryCode,
                Postcode = createCustomerEntities.AddressDetails?.FirstOrDefault()?.PostalCode,
                DateOfBirth = createCustomerEntities.DateOfBirth,
                TermsAndConditionsAccepted = createCustomerEntities.TermsAndConditionsAccepted,
                Channel = createCustomerEntities.MediaCode,
                PartnerMetadata = partnerMetadataContent

            };
        }

        internal static VredConsentRequest MapToVredMarketingConsentRequest(ConsentReceiptRequest consentReceiptRequest)
        {
            return new VredConsentRequest
            {
                Identifier = consentReceiptRequest.Emailid,
                RequestInformation = ConfigReader.Get(Constants.Environment.VRedConsentRequestToken),
                Purposes = new List<Purpose>
                {
                    new Purpose
                    {
                        Id = ConfigReader.Get(Constants.Environment.VRedConsentRequestKey1),
                        TransactionType = (consentReceiptRequest.MarketingPreference == "Y" ? Convert.ToString(VredConsentStatusEnum.CONFIRMED) : Convert.ToString(VredConsentStatusEnum.NOTGIVEN)),
                    },
                    new Purpose
                    {
                        Id = ConfigReader.Get(Constants.Environment.VRedConsentRequestKey2),
                        TransactionType = (consentReceiptRequest.MarketingPreference == "Y" ? Convert.ToString(VredConsentStatusEnum.CONFIRMED) : Convert.ToString(VredConsentStatusEnum.NOTGIVEN)),
                    }
                },
                DSDataElements = new DSDataElements
                {
                    CountryCode = consentReceiptRequest.CountryCode,
                    DateOfBirth = DateTime.Parse(consentReceiptRequest.DateOfBirth).ToString(Application.VredOneTrustDateFormat),
                    FirstName = consentReceiptRequest.FirstName,
                    LastName = consentReceiptRequest.LastName
                }
            };
        }
    }
}
