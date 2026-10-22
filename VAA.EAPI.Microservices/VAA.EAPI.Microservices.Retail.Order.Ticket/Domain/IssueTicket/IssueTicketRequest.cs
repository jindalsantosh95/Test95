using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket
{
    public class IssueTicketRequest
    {
        public Pos pos { get; set; }
        public Parties parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public List<PaymentDetails> paymentDetails { get; set; }
        public List<Customer> CustomerDetails { get; set; }
    }
}
