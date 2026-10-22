using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.BrandMS
{
    public static class BrandRequestPTCMapper
    {
        public static BrandPTCQueryRequest MapBrandMSRequest(BrandQueryRequest brandReq,IEnumerable<string> totalPtc)
        {
            return new BrandPTCQueryRequest()
            {
                BrandQueryRequest = totalPtc.Select(ptc => (BrandQueryRequest)(brandReq.Currency, brandReq.Itinerary, ptc.Split("_")[0],brandReq.PseudoCityCode,brandReq.AccountCode))
            };
        }
    }
}
