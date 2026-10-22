using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class CustomerSearchMapper
    {
        public static Entities.CustomerSearch MapEAPICustomerToSCVCustomerSearch(CreateCustomerEntities createCustomerEntities, string docType)
        {
            string UpdatedLastName = GetLastName.PopulateLastName(createCustomerEntities);
            return new Entities.CustomerSearch()
            {
                DateOfBirth = DateTimeHelper.ConvertDateTimeFormat(createCustomerEntities?.DateOfBirth, Constants.Application.EAPIEnrolmentDOBFormat, Constants.Application.EAPIEnrolmentDOBFormat),
                MatchString = MatchString.GetMatchString(createCustomerEntities),
                FirstName = createCustomerEntities?.FirstName,
                Province = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? null : !createCustomerEntities.AddressDetails.All(a => string.IsNullOrEmpty(a.Region)) ? createCustomerEntities?.AddressDetails?.First(x => x.PreferredFlag.Is("Y"))?.Region : string.Empty,
                Title = createCustomerEntities?.Title,
                CustomerId = createCustomerEntities?.CustomerId,
                Gender = createCustomerEntities?.Gender,
                MiddleName = createCustomerEntities?.MiddleName,
                Id = createCustomerEntities?.CustomerType == CustomerTypeEnum.Loyalty || createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID ? createCustomerEntities.CustomerId : Guid.NewGuid().ToString(),
                EmailAddress = createCustomerEntities?.ContactDetails?.EmailDetails?.FirstOrDefault().EmailId,
                LoyaltyMemberNumber = createCustomerEntities?.FcNumber,
                PartitionKey = createCustomerEntities?.CustomerType == CustomerTypeEnum.Loyalty || createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID ? createCustomerEntities.FcNumber : createCustomerEntities.CustomerId,
                LastName = UpdatedLastName,
                DocType = docType,
                PostCode = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? null : !createCustomerEntities.AddressDetails.All(a => string.IsNullOrEmpty(a.PostalCode)) ? createCustomerEntities?.AddressDetails?.First(x => x.PreferredFlag.Is("Y"))?.PostalCode : string.Empty,
                Country = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? null : !createCustomerEntities.AddressDetails.All(a => string.IsNullOrEmpty(a.CountryName)) ? createCustomerEntities?.AddressDetails?.First(x => x.PreferredFlag.Is("Y"))?.CountryName : string.Empty,
                SchemaVersion = Constants.Application.SchemaVersion,
                PhoneNumber = createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? null : createCustomerEntities.ContactDetails.PhoneDetails.First(x=>x.PreferredFlag == "Y").PhoneNumber,
                PhoneCountryCode = createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? null : createCustomerEntities.ContactDetails.PhoneDetails.First(x => x.PreferredFlag == "Y").CountryCode
            };
        }
    }
}
