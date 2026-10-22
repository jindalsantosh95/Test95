using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class UpgradeOfferDetails
    {
        public string Carrier { get; set; }
        public string BrandName { get; set; }
        public IEnumerable<string> Features { get; set; }
        public string FareFamily { get; set; }
        public string Image { get; set; }
        public string Refundable { get; set; }
    }
}
