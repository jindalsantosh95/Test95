using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class RetailItemMetaData
    {
        public IEnumerable<Fare> FareInformation { get; set; }
        public SeatOffer SeatInformation { get; set; }
        public BagOffer BaggageInformation { get; set; }
        public ProductOffer ProductInformation { get; set; }
    }
}
