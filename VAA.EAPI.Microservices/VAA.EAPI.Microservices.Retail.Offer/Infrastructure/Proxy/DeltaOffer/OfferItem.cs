using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferItem
    {
        public string OfferItemId { get; set; }
        public OfferResponsibleOwner Owner { get; set; }
        public string OfferItemDesc { get; set; }
        public OfferSupplier OfferSupplier { get; set; }
        public string OfferItemExpirationTimeLimitUtcTs { get; set; }
        public IEnumerable<Pricing> OfferItemPricing { get; set; }
        public bool Mandatory { get; set; }
        public IEnumerable<ProductGroup> ProductGroups { get; set; }
        public IEnumerable<string> TripIds { get; set; }
        public IEnumerable<TripAssociation> TripAssociations { get; set; }
        public IEnumerable<string> FlightSegmentIds { get; set; }
        public IEnumerable<string> PassengerIds { get; set; }
        public int RetailItemCnt { get; set; }
        public IEnumerable<RetailItem> RetailItems { get; set; }
        public string AdditionalDesc { get; set; }
    }
}
