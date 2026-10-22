using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class ReshopRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public BookingReferences BookingReferences { get; set; }
        public ReshopQueries ReshopQueries { get; set; }
        public List<ReshopOrder> ReshopOrder { get; set; }
        public List<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public List<Customer> CustomerDetails { get; set; }
        public List<Segment> SegmentDetails { get; set; }
        public PaxNameChange PaxNameChangeDetails { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
        public bool IsCancelDisruption { get; set; }
        public bool IsReshopDisruption { get; set; }
        public bool IsReshopHoldBooking { get; set; }

    }
}