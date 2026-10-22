using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class FareFamilyMapper
    {
        public static IEnumerable<searchInputsFareFamilyToPrice> GetFareFamiliesToPrice(List<string> fareFamilies)
        {
            return from fareFamily in fareFamilies where !string.IsNullOrEmpty(fareFamily) select new searchInputsFareFamilyToPrice { name = fareFamily };
        }
    }
}
