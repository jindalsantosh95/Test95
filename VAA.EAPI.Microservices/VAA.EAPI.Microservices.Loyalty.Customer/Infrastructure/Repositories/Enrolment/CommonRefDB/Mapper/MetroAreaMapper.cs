using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper
{
    public class MetroAreaMapper
    {
        public static MetroAreaDetails MapCountryCodeData(IEnumerable<ListOfValues> listOfValues, string countryCode)
        {
            if (listOfValues.Any(x => x.Name == countryCode))
            {
                return new MetroAreaDetails()
                {
                    AdditionalValue1 = listOfValues.First(x => x.Name == countryCode).AdditionalValue1,
                    Name = countryCode
                };
            }
            return null;
        }
    }
}
