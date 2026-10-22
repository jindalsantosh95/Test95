namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket
{
    public class TicketDetails
    {
        public string TicketNo { get; set; }
        public string SegmentBookingDate { get; set; }
        public string LastTransactionEventGTs { get; set; }
    }
}
