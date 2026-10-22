using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class BaggageDetails
    {
        public IEnumerable<Route> Route { get; set; }
        public IEnumerable<CabinAllowance> CabinAllowance { get; set; }
    }
}
