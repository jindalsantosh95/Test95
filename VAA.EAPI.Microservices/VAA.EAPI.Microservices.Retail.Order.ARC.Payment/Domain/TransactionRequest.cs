using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using static VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Constants.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class TransactionRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public string JourneyType { get; set; }
        public IEnumerable<EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef> AccountableDocumentRef { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
        public string TicketStockNumber { get; set; }
        public ProductType ProductType { get; set; }
        public ARCRequestType? ARCRequestType { get; set; }
        public IEnumerable<SeatDetail> SeatDetails { get; set; }
        public ARCDocument ARCDocument { get; set; }

    }
}
