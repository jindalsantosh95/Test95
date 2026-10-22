using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagOffer
    {
        public BagAllowances BagAllowances { get; set; }
        public BagCharges BagCharges { get; set; }
        public BagEmbargo BagEmbargo { get; set; }
        public Product Product { get; set; }
        public Dictionary<string, string> AdditionalDetailsMap { get; set; }
    }
}