using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class FlightSpecific
    {
        public FlightsRequest FlightsRequest { get; set; }
        public List<string> Beneficiaries { get; set; }
        public SearchCriteria SearchCriteria { get; set; }

    }
}
