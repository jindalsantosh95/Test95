using System;
using System.Linq;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public static class AccountSignInEventDataMapper
    {
        public static CloudEvent EventDataMapperVRED(AccountSignInCustomerProfile customerProfile)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Time = DateTime.UtcNow.ToString("O"),
                Data =  new
                {
                    customerId = customerProfile.CustomerId,
                    memberNumber = customerProfile.MemberNumber,
                    memberStatus = customerProfile.MemberStatus,
                    firstName = customerProfile.FirstName,
                    lastName = customerProfile.LastName,
                    emailId = customerProfile.Email,
                    dateOfBirth = customerProfile.DateOfBirth,
                    channel = customerProfile.EnrollmentChannel,
                    createUpdateTimestamp = DateTime.TryParse(customerProfile.CreatedOn, out var createdOnUtc)
                    ? createdOnUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : customerProfile.CreatedOn
                },
                Source = Constants.Application.ManageAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent EventDataMapperOCDB(AccountSignInCustomerProfile customerProfile)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinOCDB,
                Time = DateTime.UtcNow.ToString("O"),
                Data = new
                {
                    firstName = customerProfile.FirstName,
                    lastName = customerProfile.LastName,
                    fcNumber = customerProfile.MemberNumber,
                    addressDetails = customerProfile.AddressDetails?.Where(address => address.PreferredFlag == "Y").Select(address => new
                    {
                        addressLine1 = address.AddressLine1,
                        addressLine2 = address.AddressLine2,
                        addressLine3 = address.AddressLine3,
                        addressType = address.AddressType.ToString(),
                        preferredFlag = address.PreferredFlag,
                        city = address.City,
                        countryCode = address.Country,
                        region = address.Region,
                        postalCode = address.PostalCode
                    }).ToList(),
                    contactDetails = new
                    {
                        phoneDetails = customerProfile.ContactDetails?.PhoneDetails?.Where(phone => phone.PreferredFlag == "Y").Select(phone => new
                        {
                            phoneNumber = phone.PhoneNumber,
                            countryCode = phone.CountryCode,
                            phoneType = phone.PhoneType?.ToString() == "Landline" ? "Landline" : "Mobile",
                            preferredFlag = phone.PreferredFlag
                        }).ToList(),
                        emailDetails = customerProfile.ContactDetails?.EmailDetails?.Where(email => email.PreferredFlag == "Y").Select(email => new
                        {
                            emailId = email.EmailId,
                            usage = email.Usage?.ToString() == "Business" ? "Business" : "Personal",
                            preferredFlag = email.PreferredFlag
                        }).ToList()
                    }
                },
                Source = Constants.Application.ManageAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinOCDB,
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
