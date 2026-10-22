using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Mapper
{
    public class CountryDataMapper
    {
        public static List<CountryDetails> MapCountryCodeData(IEnumerable<ListOfValues> listOfValues)
        {
            List<CountryDetails> countryCodeDetailsList = new List<CountryDetails>();
            foreach (var m in listOfValues ?? Array.Empty<ListOfValues>())
            {
                CountryDetails countryCodeDetails = new CountryDetails()
                {
                    Name = m?.Name,
                    Type = m?.Type,
                    Value = m?.Value
                };
                countryCodeDetailsList.Add(countryCodeDetails);
            }
            return countryCodeDetailsList;
        }
    }
}
