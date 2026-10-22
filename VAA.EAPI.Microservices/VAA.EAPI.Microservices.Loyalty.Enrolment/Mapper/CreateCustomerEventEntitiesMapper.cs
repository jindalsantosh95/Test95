using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Entities;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.MemberStatus;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    public class CreateCustomerEventEntitiesMapper
    {
        public static CreateCustomerEventEntities CustomerEventEntitiesMapper(EnrolmentRequest enrolmentRequest, Result<Response<EnrolmentResponse>> enrolmentResponse, List<Domain.CommonReferenceDetails.CountryDetails> countryDetails,
          List<Domain.CommonReferenceDetails.PartnerStatusCodeDetails> partnerstatuscodeDetails, List<Domain.CommonReferenceDetails.PartnerProfileDetails> partnercodeDetails, List<Domain.CommonReferenceDetails.MetroAreaDetails> metroAreaDetails, string actualTitle, string abbreviatedTitle)
        {

            List<AddressDetails> addressGrp = new List<AddressDetails>();
            enrolmentRequest.CustomerDetails.AddressDetails?.ForEach(ad =>
            {
                AddressDetails addressDetail = new AddressDetails()
                {
                    AddressType = ad?.AddressType.ToString(),
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
                    AddressVerified = ad?.AddressVerified,
                    PreferredFlag = ad?.PreferredFlag,
                };

                addressGrp.Add(addressDetail);
            });
            List<Phone> ph = new List<Phone>();
            enrolmentRequest.CustomerDetails.ContactDetails?.PhoneDetails?.ForEach(pd =>
            {
                Phone phone = new Phone()
                {
                    PhoneType = pd?.PhoneType,
                    Usage = pd?.Usage,
                    CountryCode = pd?.CountryCode,
                    PhoneNumber = pd?.PhoneNumber,
                    PreferredFlag = pd?.PreferredFlag,
                    Description = pd?.Description
                };
                ph.Add(phone);
            });
            List<Email> em = new List<Email>();
            enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails?.ForEach(e =>
            {
                Email email = new Email()
                {
                    Usage = e?.Usage,
                    EmailId = e?.EmailId.ToLower(),
                    PreferredFlag = e?.PreferredFlag,
                    Description = e?.Description,
                };
                em.Add(email);
            });
            Contact contactGrp = new Contact()
            {
                EmailDetails = em,
                PhoneDetails = ph,
            };
            Phone guard_phone = new Phone()
            {
                PhoneType = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PhoneType,
                CountryCode = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.CountryCode,
                Usage = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Usage,
                Description = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.Description,
                PreferredFlag = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PreferredFlag,
                PhoneNumber = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.PhoneDetails?.PhoneNumber,
            };
            Email guard_email = new Email()
            {
                Usage = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.Usage,
                EmailId = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.EmailId,
                PreferredFlag = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.PreferredFlag,
                Description = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.EmailDetails?.Description,
            };
            Guardian guard = new Guardian()
            {
                Name = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.Name,
                DateOfBirth = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.DateOfBirth,
                CardNumber = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.CardNumber,
                GuardianTermsAndConditionsAccepted = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.GuardianDetails?.GuardianTermsAndConditionsAccepted,
                PhoneDetails = guard_phone,
                EmailDetails = guard_email,
            };
            List<Preferences> pref = new List<Preferences>();
            if (enrolmentRequest.CustomerDetails.Preferences != null)
            {
                enrolmentRequest.CustomerDetails.Preferences.ForEach(p =>
                {
                    Preferences pr = new Preferences()
                    {
                        PreferenceType = p?.PreferenceType,
                        PreferenceValue = p?.PreferenceValue,
                    };
                    pref.Add(pr);
                });
            }
            List<CustomerAttribute> customerattribute = new List<CustomerAttribute>();
            if (enrolmentRequest.CustomerDetails.CustomerAttribute != null)
            {
                enrolmentRequest.CustomerDetails.CustomerAttribute.ForEach(c =>
                {
                    CustomerAttribute attribute = new CustomerAttribute()
                    {
                        Type = c?.Type,
                        Value = c?.Value,
                    };
                    customerattribute.Add(attribute);
                });
            }

            CreateCustomerEventEntities createCustomerEventEntities = new CreateCustomerEventEntities()
            {
                FirstName = enrolmentRequest?.CustomerDetails?.FirstName,
                LastName = enrolmentRequest?.CustomerDetails?.LastName,
                MiddleName = enrolmentRequest?.CustomerDetails?.MiddleName,
                PreferredGender = enrolmentRequest?.CustomerDetails?.PreferredGender,
                PreferredName = enrolmentRequest?.CustomerDetails?.PreferredName,
                PreferredSalutation = enrolmentRequest?.CustomerDetails?.PreferredSalutation,
                PreferredPronouns = enrolmentRequest?.CustomerDetails?.PreferredPronouns,
                Gender = enrolmentRequest?.CustomerDetails?.Gender,
                Title = actualTitle,
                AbbreviatedTitle = abbreviatedTitle,
                CustomerType = enrolmentRequest?.CustomerDetails?.CustomerType.ToString(),
                DateOfBirth = enrolmentRequest?.CustomerDetails?.DateOfBirth,
                Password = enrolmentRequest?.CustomerDetails?.Password,
                AddressDetails = addressGrp,
                ContactDetails = contactGrp,
                MemberType = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.MemberType.ToString(),
                ProgramName = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.ProgramName.ToString(),
                MemberNumber = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.MemberNumber,
                FlyingClubCardNumber = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.FlyingClubCardNumber,
                Status = EnumOperations.GetEnumDescription((StatusEnum)Enum.Parse(typeof(StatusEnum), enrolmentResponse.Value.Value.Memberdetails.Status.ToString())),
                CampaignCode = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.CampaignCode,
                TestMember = (bool)enrolmentRequest?.CustomerDetails?.LoyaltyMember?.TestMember,
                MediaCode = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.MediaCode,
                EnrollmentChannel = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel,
                TermsAndConditionsAccepted = enrolmentRequest?.CustomerDetails?.CustomerType == Customer.CustomerTypeEnum.Basic ? false : (bool)enrolmentRequest?.CustomerDetails?.LoyaltyMember?.TermsAndConditionsAccepted,
                GuardianDetails = guard,
                PartnerCode = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.PartnerMember?.PartnerCode,
                StartDate = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StartDate,
                StatusEnhancementCode = enrolmentRequest?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StatusEnhancementCode,
                PartnerStatusCode = partnerstatuscodeDetails.Where(x => x.StatusCode == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.PartnerMember?.StatusEnhancementCode).Select(x => x?.StatusCode)?.FirstOrDefault(),
                PartnerMembershipMappingId = partnercodeDetails.Where(x => x.PartnerCode == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.PartnerMember?.PartnerCode).Select(x => x?.RowId)?.FirstOrDefault(),
                ReasonForTravel = enrolmentRequest?.CustomerDetails?.TravelProfile?.ReasonForTravel.ToString(),
                Preferences = pref,
                CustomerAttribute = customerattribute,
                CustomerId = enrolmentResponse.Value?.Value?.CustomerId,
                FcNumber = enrolmentResponse.Value?.Value?.Memberdetails?.FlyingClubCardNumber,
                ChannelUserId = enrolmentRequest?.ChannelUserId
            };

            return createCustomerEventEntities;
        }

    }
}
