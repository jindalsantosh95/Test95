using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class FlightSpecific
    {
        public FlightsRequest FlightsRequest { get; set; }
        public IEnumerable<string> Beneficiaries { get; set; }
        public SearchCriteria SearchCriteria { get; set; }
    }
}