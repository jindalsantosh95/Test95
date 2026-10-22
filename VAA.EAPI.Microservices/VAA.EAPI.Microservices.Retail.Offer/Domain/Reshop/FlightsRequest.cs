using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class FlightsRequest
    {
        public List<Add> Add { get; set; }
        public List<Delete> Delete { get; set; }
        public List<Retain> Retain { get; set; }
    }
}
