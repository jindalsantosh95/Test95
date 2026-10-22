namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicketOrder
{
    public class TicketDetails
    {
        public string TicketNo { get; set; }
        public string SegmentBookingDate { get; set; }
        public string LastTransactionEventGTs { get; set; }
    }
}
