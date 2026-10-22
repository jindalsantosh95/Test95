using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferDataListRetainSeatsResponse
    {
        public IEnumerable<SeatsResponse> Seats { get; set; }
    }
}
