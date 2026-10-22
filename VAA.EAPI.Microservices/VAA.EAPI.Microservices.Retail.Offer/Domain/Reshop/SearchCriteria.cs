using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class SearchCriteria
    {
        public string[] ClassPreferences { get; set; }
        public int NumberStops { get; set; }
        public string PointOfSaleCityCode { get; set; }
        public bool CheckInBaggageAllowance { get; set; } = true;
        public bool CarryOnBaggageAllowance { get; set; } = true;
        public bool RefundableOnly { get; set; }
        public string[] PermittedCarrierCode { get; set; }
        public List<DiscountCertificate> DiscountCertificate { get; set; }
        public List<CabinType> CabinTypes { get; set; }
        public List<string> FareFamilies { get; set; }
        public bool NonStopOnly { get; set; }


    }
}
