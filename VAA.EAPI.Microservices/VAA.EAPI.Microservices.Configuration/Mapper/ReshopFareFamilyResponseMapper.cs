using Newtonsoft.Json;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Mapper
{
    public class ReshopFareFamilyResponseMapper
    {
        public static List<Reshop_FareFamily> MapFareFamilyDetail(List<ReshopFareFamilyDBEntity> reshopFareFamilies)
        {
            List<Reshop_FareFamily> fareFamilies = new();
            foreach (var reshopFareFamily in reshopFareFamilies)
            {
                var fareFamily = new Reshop_FareFamily()
                {
                    CabinCode = reshopFareFamily.CABINCODE,
                    ExistingFareFamily = reshopFareFamily.EXISTING_FAREFAMILY,
                    ItaFareFamilies = JsonConvert.DeserializeObject<IEnumerable<string>>(reshopFareFamily.ITA_FAREFAMILIES)
                };
                fareFamilies.Add(fareFamily);

            }
            return fareFamilies;
        }
    }
}
