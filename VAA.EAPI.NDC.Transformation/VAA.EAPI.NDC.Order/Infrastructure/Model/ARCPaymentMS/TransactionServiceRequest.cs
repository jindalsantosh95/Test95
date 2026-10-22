using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS
{
    public class TransactionServiceRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public ProductType ProductType { get; set; }
        public string JourneyType { get; set; }
        public IEnumerable<EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
        public string TicketStockNumber { get; set; }
        public ARCRequestType? ARCRequestType { get; set; }
        public IEnumerable<SeatMS.OrderItems> SeatDetails { get; set; }
        public ARCDocument ARCDocument { get; set; }

    }
}
