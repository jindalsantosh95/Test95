using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FreeBaggageAllowance
    {
        public int AllowedBagCnt { get; set; }
        public IEnumerable<BagDescriptor> BagDescriptors { get; set; }
        public Weight Weight { get; set; }
    }
}
