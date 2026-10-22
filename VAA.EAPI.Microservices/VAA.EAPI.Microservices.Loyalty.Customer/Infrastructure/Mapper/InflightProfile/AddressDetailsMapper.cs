using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile
{
    public class AddressDetailsMapper
    {
        public static List<Address> MapAddresses(Proxy.SCVPlusProfile.Customer customerDetails, IEnumerable<CountryDetails> countryDetails)
        {
            if (customerDetails.AddressDetails != null)
            {
                Proxy.SCVPlusProfile.Address scvPrimaryAddress = customerDetails.AddressDetails.FirstOrDefault(a => a.Primary == "Y");

                if (scvPrimaryAddress != null)
                {
                    var countryList = countryDetails.Where(c => c.Value.Is(scvPrimaryAddress?.Country));

                    return new List<Address>()
                    {
                         new Address()
                        {
                            AddressLine1 = scvPrimaryAddress.AddressLine1,
                            AddressLine2 = scvPrimaryAddress.AddressLine2,
                            AddressLine3 = scvPrimaryAddress.AddressLine3,
                            AddressType = Enum.TryParse(Convert.ToString(scvPrimaryAddress.AddressType), out Address.AddressTypeEnum addressType) ? addressType : null,
                            City = scvPrimaryAddress.City,
                            CompanyName = scvPrimaryAddress.CompanyName,
                            Country = scvPrimaryAddress.Country,
                            CountryCode = !string.IsNullOrEmpty(scvPrimaryAddress.Country) ? countryList.FirstOrDefault(c => !c.Name.Is(scvPrimaryAddress.Country)).Name : string.Empty,
                            PostalCode = scvPrimaryAddress.PostalCode,
                            PreferredFlag = scvPrimaryAddress.Primary,
                            Region = scvPrimaryAddress.Region,
                            AddressVerified  = scvPrimaryAddress.QasVerified
                        }
                    };
                }

            }
            return null;
        }
    }
}