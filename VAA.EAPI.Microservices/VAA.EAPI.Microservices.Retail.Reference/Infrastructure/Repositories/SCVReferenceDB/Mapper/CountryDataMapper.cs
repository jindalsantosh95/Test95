using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper
{
    public class CountryDataMapper
    {
        public static IEnumerable<CountryDetails> MapCountryCodeData(IEnumerable<ListOfValues> listOfValues)
        {
            var countryCodeDetailsList = new List<CountryDetails>();

            foreach (var m in listOfValues ?? Array.Empty<ListOfValues>())
            {
                CountryDetails countryCodeDetails = new CountryDetails()
                {
                    Name = m.Name,
                    Type = m.Type,
                    Value = m.Value,
                    MetroArea = m.AdditionalValue1
                };
                countryCodeDetailsList.Add(countryCodeDetails);
            }
            return countryCodeDetailsList;
        }
    }
}
