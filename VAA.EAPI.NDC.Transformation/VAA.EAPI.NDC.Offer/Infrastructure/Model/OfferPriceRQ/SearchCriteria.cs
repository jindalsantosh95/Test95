using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ
{
    public class SearchCriteria
    {
        public IEnumerable<string> ClassPreferences { get; set; }
        public int NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool CheckInBaggageAllowance { get; set; } = true;
        public bool CarryOnBaggageAllowance { get; set; } = true;
        public bool RefundableOnly { get; set; }
        public IEnumerable<string> PermittedCarrierCode { get; set; }
        public IEnumerable<DiscountCertificate> DiscountCertificate { get; set; }
        public IEnumerable<CabinType> CabinTypes { get; set; }
        public IEnumerable<string> FareFamilies { get; set; }
    }
}
