using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class OfferRequest
    {
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public FlightsearchRequest FlightsearchRequest { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        public List<ServiceProduct> ServiceProducts { get; set; }
        public IEnumerable<TtlInfo> TtlInfo { get; set; }
    }
}