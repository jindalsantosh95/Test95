
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData
{
    public class RetriveTicketRequest
    {
        public string ETicketNumber { get; set; }
        public bool IncludeAssociatedEmds { get; set; }
    }
}
