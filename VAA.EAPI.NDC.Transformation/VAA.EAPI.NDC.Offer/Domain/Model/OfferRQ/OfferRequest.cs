using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.NDC.Offer.Domain.Model.OfferRQ;
using VAA.EAPI.NDC.Offer.Domain.Models.VAA;

namespace VAA.EAPI.NDC.Offer.Domain.Models.OfferRQ
{
    public class OfferRequest
    {
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightSearchRequest FlightsearchRequest { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<ServiceProduct> ServiceProducts { get; set; }
        public IEnumerable<TtlInfo> TtlInfo { get; set; }
    }
}
