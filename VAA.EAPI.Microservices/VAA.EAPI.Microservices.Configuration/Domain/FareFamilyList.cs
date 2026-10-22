using Newtonsoft.Json;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class FareFamilyList
    {
        public string CabinCode { get; set; }
        public IEnumerable<ServiceTaxonomyDTO> ServiceTaxonomy { get; set; }
        public string FareTypeCode { get; set; }
        public IEnumerable<string> FareFamily { get; set; }

        public static explicit operator FareFamilyList(FareFamilyListDbEntity fareFamilyListDbEntity)
        {
            return new FareFamilyList()
            {
                CabinCode = fareFamilyListDbEntity.CabinCode,
                FareTypeCode = fareFamilyListDbEntity.FareTypeCode,
                FareFamily = JsonConvert.DeserializeObject<IEnumerable<string>>(fareFamilyListDbEntity.FareFamily),
                ServiceTaxonomy = JsonConvert.DeserializeObject<IEnumerable<ServiceTaxonomyDTO>>(fareFamilyListDbEntity.ServiceTaxonomy),
            };
        }
    }
}
