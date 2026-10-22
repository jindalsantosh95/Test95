using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.FareFamily
{
    public class FareFamilyList
    {
        public string CabinCode { get; set; }
        public IEnumerable<ServiceTaxonomyDTO> ServiceTaxonomy { get; set; }
        public string FareTypeCode { get; set; }
        public IEnumerable<string> FareFamily { get; set; }

        public static explicit operator FareFamilyList(FareFamilyListDbEntity fareFamilyIEnumerableDbEntity)
        {
            return new FareFamilyList()
            {
                CabinCode = fareFamilyIEnumerableDbEntity.CabinCode,
                FareTypeCode = fareFamilyIEnumerableDbEntity.FareTypeCode,
                FareFamily = JsonConvert.DeserializeObject<IEnumerable<string>>(fareFamilyIEnumerableDbEntity.FareFamily),
                ServiceTaxonomy = JsonConvert.DeserializeObject<IEnumerable<ServiceTaxonomyDTO>>(fareFamilyIEnumerableDbEntity.ServiceTaxonomy),
            };
        }
    }
}
