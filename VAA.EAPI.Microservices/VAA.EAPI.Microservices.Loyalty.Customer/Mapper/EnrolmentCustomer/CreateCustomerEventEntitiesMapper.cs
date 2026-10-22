using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class CreateCustomerEventEntitiesMapper
    {
        public static CreateCustomerEventResponse CustomerEventResponseMapper(CreateCustomerEntities createCustomerEventEntities, Result<Response<TierInfo>> tierDetails)
        {

            List<EmailDetails> emailDetails = new List<EmailDetails>();
            createCustomerEventEntities?.ContactDetails.EmailDetails.ForEach(ed =>
            {
                EmailDetails emailDetail = new()
                {
                    Usage = ed?.Usage,
                    EmailId = ed?.EmailId,
                    Description = ed?.Description,
                    PreferredFlag = ed?.PreferredFlag,
                    Valid = ed?.Valid
                };
                emailDetails.Add(emailDetail);
            });
            List<PhoneDetails> phoneDetails = new List<PhoneDetails>();
            createCustomerEventEntities?.ContactDetails.PhoneDetails?.ForEach(pd =>
            {
                PhoneDetails phoneDetail = new()
                {
                    Usage = pd?.Usage,
                    PhoneNumber = pd?.PhoneNumber,
                    CountryCode = pd?.CountryCode,
                    PhoneType = pd?.PhoneType,
                    Description = pd?.Description,
                    PreferredFlag = pd?.PreferredFlag
                };
                phoneDetails.Add(phoneDetail);
            });

            ContactDetails contactDetails = new() { EmailDetails = emailDetails, PhoneDetails = phoneDetails };

            List<AddressDetail> addressGrp = new();
            createCustomerEventEntities?.AddressDetails?.ForEach(ad =>
            {
                AddressDetail addressDetail = new()
                {
                    AddressType = ad?.AddressType,
                    AddressLine1 = ad?.AddressLine1,
                    AddressLine2 = ad?.AddressLine2,
                    AddressLine3 = ad?.AddressLine3,
                    City = ad?.City,
                    CompanyName = ad?.CompanyName,
                    PostalCode = ad?.PostalCode,
                    Region = ad?.Region,
                    CountryCode = ad?.CountryCode,
                    CountryName = ad?.CountryName,
                    PreferredFlag = ad?.PreferredFlag,
                    AddressVerified = (bool)(ad?.AddressVerified),
                    MetroArea = ad.MetroArea
                };

                addressGrp.Add(addressDetail);
            });
            return new CreateCustomerEventResponse()
            {
                FirstName = createCustomerEventEntities?.FirstName,
                LastName = createCustomerEventEntities?.LastName,
                MiddleName = createCustomerEventEntities?.MiddleName,
                Gender = createCustomerEventEntities?.Gender,
                Title = createCustomerEventEntities?.Title,
                AbbreviatedTitle = createCustomerEventEntities?.AbbreviatedTitle,
                CustomerType = Convert.ToString(createCustomerEventEntities?.CustomerType),
                DateOfBirth = createCustomerEventEntities?.DateOfBirth,
                MemberType = createCustomerEventEntities?.MemberType,
                FlyingClubCardNumber = createCustomerEventEntities?.FlyingClubCardNumber,
                CampaignCode = createCustomerEventEntities?.CampaignCode,
                MediaCode = createCustomerEventEntities?.MediaCode,
                Status = createCustomerEventEntities?.Status,
                TermsAndConditionsAccepted = (bool)(createCustomerEventEntities?.TermsAndConditionsAccepted),
                TestMember = createCustomerEventEntities?.TestMember?.ToString() ?? "false",
                EnrollmentChannel = createCustomerEventEntities?.EnrollmentChannel,
                CustomerId = createCustomerEventEntities?.CustomerId,
                FcNumber = createCustomerEventEntities?.FcNumber,
                TierEndDate = DateTime.Now.AddYears(1).GetLastDayOfMonth().ToString(Constants.Application.ISODateTimeFormat),
                TierStartDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                CurrentTier = TierCode.Red.ToString(),
                ApprovalDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                AddressDetails = addressGrp,
                ContactDetails = contactDetails
            };
        }


    }
}
