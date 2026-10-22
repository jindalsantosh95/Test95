using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatPreference
    {
        public string BrandId { get; set; }
        public IEnumerable<Seat> SeatTypes { get; set; }
    }
}
