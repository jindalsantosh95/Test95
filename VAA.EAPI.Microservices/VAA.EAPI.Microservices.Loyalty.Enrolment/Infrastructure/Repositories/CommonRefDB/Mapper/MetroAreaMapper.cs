using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Mapper
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
