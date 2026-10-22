using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class SearchCriteria
    {
        public IEnumerable<string> ClassPreferences { get; set; }
        public int? NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool? CheckInBaggageAllowance { get; set; }
        public bool? CarryOnBaggageAllowance { get; set; }
        public bool? RefundableOnly { get; set; }
        public IEnumerable<string> PermittedCarrierCode { get; set; }
        public IEnumerable<DiscountCertificate> DiscountCertificate { get; set; }
        public IEnumerable<ReshopCabinType> CabinTypes { get; set; }
        public IEnumerable<string> FareFamilies { get; set; }
        public bool? NonStopOnly { get; set; }
    }
}
