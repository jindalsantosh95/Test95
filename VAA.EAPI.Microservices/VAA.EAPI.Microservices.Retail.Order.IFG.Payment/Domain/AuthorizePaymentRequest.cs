using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    public class AuthorizePaymentRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public string TrackId { get; set; }
        public IEnumerable<TrackIdHistory> TrackIdsHistory { get; set; }
        public IEnumerable<Payment> PaymentDetails { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public ProductType ProductType { get; set; }
        public string JourneyType { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
        public bool IsRfc {  get; set; }
        public bool ExchangeRefund { get; set; }
        public IEnumerable<OrderHistory> OrderHistory { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public bool IsChangeReshopHoldBooking { get; set; }

    }
}
