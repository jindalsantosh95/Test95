using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightOfferCriteria
    {
        public bool InventoryHeld { get; set; }
        public IEnumerable<string> PassengerIds { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public string SoldCountryCode { get; set; }
        public string SortableOptionId { get; set; }
        public string SortByBrandId { get; set; }
        public bool BundleOffer { get; set; }
        public bool CalendarSearch { get; set; }
        public string CurrentTripIndexId { get; set; }
        public bool RetrieveNextAvailableOffer { get; set; }
        public string SelectedOfferId { get; set; }
        public IEnumerable<SearchOriginDestination> SearchOriginDestination { get; set; }
        public IEnumerable<SpecificOriginDestination> SpecificOriginDestination { get; set; }
        public Dictionary<string, string> AdditionalCriteriaMap { get; set; }
        public string BookingCosCode { get; set; }
        public string IssuedCountryCode { get; set; }
    }
}
