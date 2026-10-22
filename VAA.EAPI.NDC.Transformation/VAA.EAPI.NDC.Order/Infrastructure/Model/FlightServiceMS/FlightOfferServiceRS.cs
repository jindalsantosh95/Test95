using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS
{
    public class FlightOfferServiceRS
    {
        public FlightServiceResponse ServiceListResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
