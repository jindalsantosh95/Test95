using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper
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
