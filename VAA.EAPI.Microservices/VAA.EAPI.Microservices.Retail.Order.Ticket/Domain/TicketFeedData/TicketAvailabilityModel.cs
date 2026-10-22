
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData
{
    public class TicketAvailabilityModel
    {
        public string DocType { get; set; }

        public string Id { get; set; }

        public string TkDcUpdtSqlNb { get; set; }

        public string ETag { get; set; }

        public string Ts { get; set; }

        public int? Ttl { get; set; } = null;
    }
}
