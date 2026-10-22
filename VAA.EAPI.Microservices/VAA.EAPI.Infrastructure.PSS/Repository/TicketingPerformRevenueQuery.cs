using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingPerformRevenueQuery : IRequest<BookingDetails>
    {
        public string _SessionId { get; set; }
        public List<PaymentDetails> _PaymentsDetails { get; set; }
        public List<Customer> _Customer { get; set; }

        public TicketingPerformRevenueQuery(string sessionId, List<PaymentDetails> paymentsDetails, List<Customer> customer)
        {
            _SessionId = sessionId;
            _PaymentsDetails = paymentsDetails;
            _Customer = customer;

        }
    }
}
