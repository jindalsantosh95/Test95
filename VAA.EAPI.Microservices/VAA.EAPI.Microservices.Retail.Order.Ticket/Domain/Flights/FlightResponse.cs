using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights
{
    public class FlightResponse
    {
        public string ContactEmail { get; set; }
        public string FFPNumber { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
    }       
}
