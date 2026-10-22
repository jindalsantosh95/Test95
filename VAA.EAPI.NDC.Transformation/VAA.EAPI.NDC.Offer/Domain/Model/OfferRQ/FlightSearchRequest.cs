using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Offer.Domain.Models.OfferRQ
{
    public class FlightSearchRequest
    {
        public IEnumerable<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public bool BundleOffer { get; set; }
        public bool CalendarSearch { get; set; }
        public bool NonStopOnly { get; set; }
        public string CurrentTripIndexId { get; set; }
        public IEnumerable<string> ClassPreferences { get; set; }
        public int NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool CheckInBaggageAllowance { get; set; }
        public bool CarryOnBaggageAllowance { get; set; }
        public bool RefundableOnly { get; set; }
        public IEnumerable<string> PermittedCarrierCode { get; set; }
        public IEnumerable<DiscountCertificate> DiscountCertificate { get; set; }
        public IEnumerable<CabinType> CabinTypes { get; set; }
        public IEnumerable<string> FareFamilies { get; set; }
    }
}
