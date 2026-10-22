using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class CapturePaymentRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<Payment> PaymentDetails { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public ProductType ProductType { get; set; }
        public string JourneyType { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }        
        public string Status { get; set; }
    }
}