using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class FlightSearchRequest
    {
        public IEnumerable<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public bool BundleOffer { get; set; }
        public bool CalendarSearch { get; set; }
        public bool FlexiDateSearch { get; set; }
        public string PromoCode { get; set; }
        public bool AwardSearch { get; set; }
        public bool NonStopOnly { get; set; }
        public string CurrentTripIndexId { get; set; }
        public IEnumerable<string> ClassPreferences { get; set; }
        public int NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool CheckInBaggageAllowance { get; set; }
        public bool CarryOnBaggageAllowance { get; set; }
        public bool RefundableOnly { get; set; }
        public string BrandId { get; set; }
        public IEnumerable<string> PermittedCarrierCode { get; set; }
        public IEnumerable<DiscountCertificate> DiscountCertificate { get; set; }
        public IEnumerable<CabinType> CabinTypes { get; set; }
        public IEnumerable<string> FareFamilies { get; set; }
        public string OfferID { get; set; }
    }
}