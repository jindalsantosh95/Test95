using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatOffer
    {
        public IEnumerable<Seat> SeatDetails { get; set; }
        public Product AncillaryProduct { get; set; }
    }
}
