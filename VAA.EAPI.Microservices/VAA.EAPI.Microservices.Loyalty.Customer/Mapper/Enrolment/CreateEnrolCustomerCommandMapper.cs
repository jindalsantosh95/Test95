using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using CustomerTypeEnum = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.EnrolCustomerDetails.CustomerTypeEnum;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.Enrolment
{
    public class CreateEnrolCustomerCommandMapper
    {
        public static EnrolCustomerCommand CreateEnrolCustomerCommand(EnrolmentRequestCommand enrolmentRequest, Result<Response<EnrolmentResponse>> enrolmentResponse, List<CountryDetails> countryDetails,
          List<PartnerStatusCodeDetails> partnerstatuscodeDetails, List<PartnerProfileDetails> partnercodeDetails, List<MetroAreaDetails> metroAreaDetails, string actualTitle, string abbreviatedTitle)
        {

            List<AddressDetail> addressGrp = new List<AddressDetail>();
            enrolmentRequest?.Data?.CustomerDetails.AddressDetails?.ToList().ForEach(ad =>
            {
                AddressDetail addressDetail = new AddressDetail()
                {
                    AddressLine1 = ad?.AddressLine1,
                    AddressLine2 = ad?.AddressLine2,
                    AddressLine3 = ad?.AddressLine3,
                    CompanyName = ad?.CompanyName,
                    City = ad?.City,
                    PostalCode = ad?.PostalCode,
                    Region = ad?.Region,
                    CountryCode = ad?.Country,
                    CountryName = !string.IsNullOrEmpty(ad?.Country) ? countryDetails.FirstOrDefault(x => x.Name == ad?.Country).Value : string.Empty,
                    MetroArea = !string.IsNullOrEmpty(ad?.Country) ? metroAreaDetails.FirstOrDefault(x => x.Name == ad?.Country).AdditionalValue1 : string.Empty,
                    PreferredFlag = ad?.PreferredFlag,
                    AddressVerified = ad.AddressVerified,
                    AddressType = (AddressDetail.AddressTypes?)(ad?.AddressType)
                };

                addressGrp.Add(addressDetail);
            });
            List<PhoneDetails> phDtls = new List<PhoneDetails>();
            enrolmentRequest.Data.CustomerDetails.ContactDetails?.PhoneDetails?.ForEach(pd =>
            {
                PhoneDetails phone = new PhoneDetails()
                {
                    PhoneType = (PhoneDetails.PhoneTypes?)(pd?.PhoneType),
                    Usage = (PhoneDetails.Usages?)(pd?.Usage),
                    CountryCode = pd?.CountryCode,
                    PhoneNumber = pd?.PhoneNumber,
                    PreferredFlag = pd?.PreferredFlag,
                    Description = pd?.Description
                };
                phDtls.Add(phone);
            });
            List<EmailDetails> emDtls = new List<EmailDetails>();
            enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails?.ForEach(e =>
            {
                EmailDetails email = new EmailDetails()
                {
                    Usage = (EmailDetails.Usages?)e?.Usage,
                    EmailId = e?.EmailId.ToLowerInvariant(),
                    PreferredFlag = e?.PreferredFlag,
                    Description = e?.Description,
                    Valid = e?.Valid,
                };
                emDtls.Add(email);
            });
            ContactDetails contactGrp = new ContactDetails()
            {
                EmailDetails = emDtls,
                PhoneDetails = phDtls
            };
            PhoneDetails guard_phone = new PhoneDetails()
            {
                PhoneType = (PhoneDetails.PhoneTypes?)(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PhoneType),
                CountryCode = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.CountryCode,
                Usage = (PhoneDetails.Usages?)(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Usage),
                Description = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Description,
                PreferredFlag = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PreferredFlag,
                PhoneNumber = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PhoneNumber,
            };
            EmailDetails guard_email = new EmailDetails()
            {
                Usage = (EmailDetails.Usages?)(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.Usage),
                EmailId = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.EmailId,
                PreferredFlag = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.PreferredFlag,
                Description = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.Description,
            };
            GuardianDetails guard = new GuardianDetails()
            {
                Name = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.Name,
                DateOfBirth = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.DateOfBirth,
                CardNumber = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.CardNumber,
                GuardianTermsAndConditionsAccepted = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted,
                PhoneDetails = guard_phone,
                EmailDetails = guard_email,
            };
            List<Domain.Entities.Preferences> pref = new List<Domain.Entities.Preferences>();
            if (enrolmentRequest?.Data?.CustomerDetails?.Preferences != null)
            {
                enrolmentRequest?.Data?.CustomerDetails?.Preferences.ToList().ForEach(p =>
                {
                    Domain.Entities.Preferences pr = new Domain.Entities.Preferences()
                    {
                        PreferenceType = (Domain.Entities.Preferences.PreferenceTypes?)(p?.PreferenceType),
                        PreferenceValue = p?.PreferenceValue,
                    };
                    pref.Add(pr);
                });
            }
            List<Domain.Entities.CustomerAttribute> customerattribute = new List<Domain.Entities.CustomerAttribute>();
            if (enrolmentRequest?.Data?.CustomerDetails?.CustomerAttribute != null)
            {
                enrolmentRequest?.Data?.CustomerDetails?.CustomerAttribute.ToList().ForEach(c =>
                {
                    Domain.Entities.CustomerAttribute attribute = new Domain.Entities.CustomerAttribute()
                    {
                        Type = (Domain.Entities.CustomerAttribute.Types?)(c?.Type),
                        Value = c?.Value,
                    };
                    customerattribute.Add(attribute);
                });
            }

            EnrolCustomerCommand createCustomerEventEntities = new EnrolCustomerCommand()
            {
                Id = enrolmentRequest.Id,
                FirstName = enrolmentRequest?.Data?.CustomerDetails?.FirstName,
                LastName = enrolmentRequest?.Data?.CustomerDetails?.LastName,
                MiddleName = enrolmentRequest?.Data?.CustomerDetails?.MiddleName,
                PreferredGender = enrolmentRequest?.Data?.CustomerDetails?.PreferredGender,
                PreferredName = enrolmentRequest?.Data?.CustomerDetails?.PreferredName,
                PreferredSalutation = enrolmentRequest?.Data?.CustomerDetails?.PreferredSalutation,
                PreferredPronouns = enrolmentRequest?.Data?.CustomerDetails?.PreferredPronouns,
                Gender = enrolmentRequest?.Data?.CustomerDetails?.Gender,
                Title = actualTitle,
                AbbreviatedTitle = abbreviatedTitle,
                CustomerType = (Domain.Enums.CustomerTypeEnum?)(enrolmentRequest?.Data?.CustomerDetails?.CustomerType),
                DateOfBirth = enrolmentRequest?.Data?.CustomerDetails?.DateOfBirth,
                Password = enrolmentRequest?.Data?.CustomerDetails?.Password,
                AddressDetails = addressGrp,
                ContactDetails = contactGrp,
                MemberType = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.MemberType.ToString(),
                ProgramName = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.ProgramName.ToString(),
                MemberNumber = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.MemberNumber,
                FlyingClubCardNumber = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.FlyingClubCardNumber,
                Status = EnumOperations.GetEnumDescription((StatusEnum)Enum.Parse(typeof(StatusEnum), enrolmentResponse.Value.Value.Memberdetails.Status.ToString())),
                CampaignCode = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.CampaignCode,
                TestMember = (bool)enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.TestMember,
                MediaCode = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.MediaCode,
                EnrollmentChannel = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel,
                TermsAndConditionsAccepted = enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic ? false : (bool)enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.TermsAndConditionsAccepted,
                GuardianDetails = guard,
                PartnerCode = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.PartnerMember?.PartnerCode,
                StartDate = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StartDate,
                StatusEnhancementCode = enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StatusEnhancementCode,
                PartnerStatusCode = partnerstatuscodeDetails.Where(x => x.StatusCode == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StatusEnhancementCode).Select(x => x?.StatusCode)?.FirstOrDefault(),
                PartnerMembershipMappingId = partnercodeDetails.Where(x => x.PartnerCode == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.PartnerMember?.PartnerCode).Select(x => x?.RowId)?.FirstOrDefault(),
                ReasonForTravel = enrolmentRequest?.Data?.CustomerDetails?.TravelProfile?.ReasonForTravel.ToString(),
                Preferences = pref,
                CustomerAttribute = customerattribute,
                CustomerId = enrolmentResponse.Value?.Value?.CustomerId,
                FcNumber = enrolmentResponse.Value?.Value?.Memberdetails?.FlyingClubCardNumber,
                ChannelUserId = enrolmentRequest?.Data?.ChannelUserId
            };

            return createCustomerEventEntities;
        }

    }
}
