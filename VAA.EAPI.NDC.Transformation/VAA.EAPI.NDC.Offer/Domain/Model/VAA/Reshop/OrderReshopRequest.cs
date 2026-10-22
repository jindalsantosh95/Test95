using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class OrderReshopRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public BookingReference BookingReferences { get; set; }
        public ReshopQueries ReshopQueries { get; set; }
        public IEnumerable<ReshopOrder> ReshopOrder { get; set; }
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public PaxNameChange PaxNameChangeDetails { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
        public bool IsCancelDisruption { get; set; }
        public bool IsReshopDisruption { get; set; }
        public bool IsReshopHoldBooking { get; set;  }
    }
}