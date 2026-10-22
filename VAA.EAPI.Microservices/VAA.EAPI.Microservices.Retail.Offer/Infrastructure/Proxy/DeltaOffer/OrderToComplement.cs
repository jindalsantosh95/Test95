using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OrderToComplement
    {
        public string OrderId { get; set; }
        public IEnumerable<OrderContext> OrderContextList { get; set; }
    }
}
