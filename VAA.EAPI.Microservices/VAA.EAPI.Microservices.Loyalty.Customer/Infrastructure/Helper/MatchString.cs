using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper
{
    public class MatchString
    {
        public static string GetMatchString(CreateCustomerEntities createCustomerEntities)
        {
            string firstName = createCustomerEntities?.FirstName.ToUpper();
            string lastName = GetLastName.PopulateLastName(createCustomerEntities).ToUpper();
            string streetName = createCustomerEntities?.AddressDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.AddressLine1.ToUpper();
            string postCode = createCustomerEntities?.AddressDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.PostalCode?.ToUpper();
            string countryCode = createCustomerEntities?.AddressDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.CountryCode;
            string DOB = string.IsNullOrEmpty(createCustomerEntities?.DateOfBirth) ? null : DateTime.Parse(createCustomerEntities?.DateOfBirth).ToString(Constants.Application.MatchStringDateFormat);
            return Utility.GenerateMatchString(createCustomerEntities.CustomerType.ToString(), streetName, postCode, firstName, lastName, countryCode, DOB);
        }

        public static string GetMatchString(Customers customerDoc, string countryCode, string channelId)
        {
            string firstName = customerDoc?.FirstName.ToUpper();
            string lastName = customerDoc?.LastName.ToUpper();
            string streetName = customerDoc?.AddressDetails?.FirstOrDefault(x => x.Primary == "Y")?.AddressLine1.ToUpper();
            string postCode = customerDoc?.AddressDetails?.FirstOrDefault(x => x.Primary == "Y")?.PostCode?.ToUpper();
            string DOB = string.IsNullOrEmpty(customerDoc?.DateOfBirth) ? null : DateTime.Parse(customerDoc?.DateOfBirth).ToString(Constants.Application.MatchStringDateFormat);

            return Utility.GenerateMatchString(channelId, streetName, postCode, firstName, lastName, countryCode, DOB);
        }
    }
}
