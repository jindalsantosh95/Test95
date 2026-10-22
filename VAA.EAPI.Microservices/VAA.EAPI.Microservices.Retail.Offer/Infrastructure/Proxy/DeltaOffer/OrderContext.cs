using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OrderContext
    {
        public string OrderItemId { get; set; }
        public IEnumerable<string> OriginalTripIds { get; set; }
        public IEnumerable<string> OriginalFlightSegmentIds { get; set; }
    }
}
