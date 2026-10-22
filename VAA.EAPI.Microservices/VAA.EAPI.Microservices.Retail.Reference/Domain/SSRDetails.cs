using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class SSRDetails
    {
        public string SSR { get; set; }
        public string Description { get; set; }
        public string RFICCcode { get; set; }
        public string RFISCCode { get; set; }
        public string OwnerCode { get; set; }
        public IEnumerable<PriceDetail> PriceDetails { get; set; }
    }
}
