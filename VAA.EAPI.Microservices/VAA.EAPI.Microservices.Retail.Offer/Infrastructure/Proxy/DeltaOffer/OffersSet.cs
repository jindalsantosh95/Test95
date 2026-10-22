using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OffersSet
    {
        public IEnumerable<Offer> Offers { get; set; }
        public string TripId { get; set; }
        public string OriginAirportCode { get; set; }
        public string OffersSetGroupId { get; set; }
        public string ItineraryDepartureDate { get; set; }
        public Dictionary<string, string> AdditionalSetMap { get; set; }
        public AdditionalOfferSetProperties AdditionalOfferSetProperties { get; set; }
    }
}
