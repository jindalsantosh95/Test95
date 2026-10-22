using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS
{
    public class FlightServiceResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
