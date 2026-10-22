using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain
{
    public class IssueTicketRequest
    {
        public Pos Pos { get; set; }
        public Parties parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public IEnumerable<SelectedOffer> SelectedOffers { get; set; }
        public string RequestType { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<BaggageAllowances> BaggageAllowances { get; set; }
    }
}
