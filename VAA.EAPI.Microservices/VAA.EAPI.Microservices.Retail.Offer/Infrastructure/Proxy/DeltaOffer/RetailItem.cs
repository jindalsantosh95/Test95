using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class RetailItem
    {
        public string RetailItemDefinitionId { get; set; }
        public IEnumerable<string> TripIds { get; set; }
        public IEnumerable<string> FlightSegmentIds { get; set; }
        public IEnumerable<string> PassengerRefIds { get; set; }
        public bool HoldRequired { get; set; }
        public IEnumerable<Pricing> RetailItemPricing { get; set; }
        public RetailItemMetaData RetailItemMetaData { get; set; }
        public Link RetailItemMetaDataLink { get; set; }
    }
}
