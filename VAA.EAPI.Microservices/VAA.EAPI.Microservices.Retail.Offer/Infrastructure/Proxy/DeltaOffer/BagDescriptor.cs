using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagDescriptor
    {
        public string BagDescriptorName { get; set; }
        public int BagCnt { get; set; }
        public IEnumerable<string> BagDescriptorDesc { get; set; }
        public string BagDescriptorSubCode { get; set; }
    }
}
