using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferSearchCriteria
    {
        public IEnumerable<ProductCategory> ProductGroups { get; set; }
        public PointOfSale PointOfSale { get; set; }
        public Party Sender { get; set; }
        public OffersCriteria OffersCriteria { get; set; }
        public IEnumerable<Passenger> Customers { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<FlightSegment> CustomerItinerarySegmentList { get; set; }
    }
}
