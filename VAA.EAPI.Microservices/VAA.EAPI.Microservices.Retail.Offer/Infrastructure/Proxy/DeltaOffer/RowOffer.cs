using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class RowOffer : SeatRow
    {
        public IEnumerable<SeatOfferItem> Offer { get; set; }
    }
}
