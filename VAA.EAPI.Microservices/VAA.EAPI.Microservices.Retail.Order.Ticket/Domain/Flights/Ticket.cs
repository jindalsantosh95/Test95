using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights
{
    public class Ticket
    {
        public string TicketNumber { get; set; }
        public DateTime? TicketIssueDate { get; set; }
        public string SearchCriteria { get; set; }
        public IEnumerable<Flight> Flights { get; set; }
    }
}
