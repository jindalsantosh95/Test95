using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatUpgradePreference
    {
        public string BrandId { get; set; }
        public string UpgradeOptInCode { get; set; }
        public IEnumerable<Seat> SeatTypes { get; set; }
    }
}
