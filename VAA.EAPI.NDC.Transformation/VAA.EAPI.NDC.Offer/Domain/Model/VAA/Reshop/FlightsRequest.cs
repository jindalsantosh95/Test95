using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class FlightsRequest
    {
        public IEnumerable<Add> Add { get; set; }
        public IEnumerable<Delete> Delete { get; set; }
        public IEnumerable<Retain> Retain { get; set; }
    }
}
