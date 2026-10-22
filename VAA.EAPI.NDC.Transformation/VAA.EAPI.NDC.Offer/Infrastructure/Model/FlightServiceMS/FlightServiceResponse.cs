using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.FlightServiceMS
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
