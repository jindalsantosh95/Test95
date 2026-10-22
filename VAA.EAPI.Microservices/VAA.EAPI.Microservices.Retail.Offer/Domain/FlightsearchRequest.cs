using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class FlightsearchRequest
    {
        public List<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public bool BundleOffer { get; set; }
        public bool CalendarSearch { get; set; }
        public bool FlexiDateSearch { get; set; }
        public bool NonStopOnly { get; set; }
        public string CurrentTripIndexId { get; set; }
        public List<string> ClassPreferences { get; set; }
        public int NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool CheckInBaggageAllowance { get; set; }
        public bool CarryOnBaggageAllowance { get; set; }
        public bool RefundableOnly { get; set; }
        public List<string> PermittedCarrierCode { get; set; }
        public List<DiscountCertificate> DiscountCertificate { get; set; }
        public List<CabinType> CabinTypes { get; set; }
        public List<string> FareFamilies { get; set; }
        public string OfferID { get; set; }
        public string BrandId { get; set; }
        public string PromoCode { get; set; }
        public bool AwardSearch { get; set; }
    }
}
