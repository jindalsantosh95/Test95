using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatDisplay
    {
        public string CabinCode { get; set; }
        public IEnumerable<SeatColumn> Columns { get; set; }
        public SeatRowRange Rows { get; set; }
    }
}
