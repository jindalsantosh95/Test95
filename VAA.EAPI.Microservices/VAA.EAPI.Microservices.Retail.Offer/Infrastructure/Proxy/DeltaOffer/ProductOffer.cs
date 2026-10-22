using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class ProductOffer
    {
        public Product Product { get; set; }
        public BagAllowances BagAllowances { get; set; }
        public BagCharges BagCharges { get; set; }
        public Embargo Embargo { get; set; }
        public Dictionary<string, string> AdditionalDetailsMap { get; set; }
    }
}
