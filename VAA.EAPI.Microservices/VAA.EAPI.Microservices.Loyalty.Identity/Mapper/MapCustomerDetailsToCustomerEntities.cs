using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public static class MapCustomerDetailsToCustomerEntities
    {
        public static CreateCustomerEntities MaptoCreateCustomerEntities(CustomerAccountDetails custDetails)
        {
            ArgumentNullException.ThrowIfNull(custDetails);
            ArgumentNullException.ThrowIfNull(custDetails.LoyaltyMember);

            CreateCustomerEntities createCustomerEntities = new CreateCustomerEntities()
            {
                FcNumber = custDetails?.FcNumber,
                FirstName = custDetails?.FirstName,
                LastName = custDetails?.LastName,
                CustomerId = custDetails?.CustomerId,
                EnrollmentChannel = custDetails.LoyaltyMember.EnrollmentChannel,
                TestMember = false,
                TermsAndConditionsAccepted = custDetails?.LoyaltyMember?.TermsAndConditionsAccepted,
                AddressDetails = new System.Collections.Generic.List<AddressDetail>(),
                DateOfBirth = custDetails?.DateOfBirth,
                Gender = custDetails?.Gender,
                MemberType = custDetails?.LoyaltyMember?.MemberType.ToString(),
                MediaCode = custDetails?.LoyaltyMember?.MediaCode,
                TierEndDate = DateTimeHelper.GetLastDayOfMonth(DateTime.Now.AddYears(1)).ToString(Constants.Application.ISODateTimeFormat),
                TierStartDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                CurrentTier = TierCode.Red.ToString(),
                ApprovalDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                FlyingClubCardNumber = custDetails?.LoyaltyMember?.FlyingClubCardNumber,
                Status = custDetails?.Status,
                Preferences = custDetails.Preferences,
                GuardianDetails = custDetails?.LoyaltyMember?.GuardianDetails
            };

            createCustomerEntities.CustomerType = custDetails.CustomerType.ToString();

            createCustomerEntities.ContactDetails = new ContactDetails() { EmailDetails = new(), PhoneDetails = new() };

            if (custDetails.AddressDetails != null)
            {
                custDetails.AddressDetails.ForEach(item =>
                {
                    createCustomerEntities.AddressDetails.Add(new AddressDetail()
                    {
                        AddressLine1 = item.AddressLine1,
                        AddressLine2 = item.AddressLine2,
                        AddressLine3 = item.AddressLine3,
                        City = item.City,
                        PostalCode = item.PostalCode,
                        PreferredFlag = item.PreferredFlag,
                        CountryCode = item.Country,
                        CountryName = item.Country,
                        AddressType = item.AddressType?.ToString(),
                        Region = item.Region,
                        AddressVerified = item.AddressVerified
                    });
                });
            }

            if (custDetails.ContactDetails?.EmailDetails != null)
            {
                custDetails.ContactDetails?.EmailDetails.ForEach(item =>
                {
                    createCustomerEntities.ContactDetails.EmailDetails.Add(new EmailDetail()
                    {
                        EmailId = item.EmailId.ToLowerInvariant(),
                        Usage = item.Usage?.ToString(),
                        Description = item.Description,
                        PreferredFlag = item.PreferredFlag,
                        Valid = item.Valid
                    });

                });
            }

            if (custDetails.ContactDetails?.PhoneDetails != null)
            {
                custDetails.ContactDetails?.PhoneDetails.ForEach(item =>
                {
                    createCustomerEntities.ContactDetails.PhoneDetails.Add(new PhoneDetail()
                    {
                        PhoneNumber = item.PhoneNumber,
                        PhoneType = item.PhoneType?.ToString(),
                        Usage = item.Usage?.ToString(),
                        Description = item.Description,
                        PreferredFlag = item.PreferredFlag,
                        CountryCode = item.CountryCode
                    });
                });
            }

            return createCustomerEntities;
        }
    }
}
