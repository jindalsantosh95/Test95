using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily
{
    public class FareFamilies
    {
        public string CabinCode { get; set; }
        public IEnumerable<ServiceTaxonomy> ServiceTaxonomy { get; set; }
        public string FareTypeCode { get; set; }
        public IEnumerable<string> FareFamily { get; set; }
    }
}
