using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class Fulfillment
    {
        public List<Ticket> Tickets { get; set; }
        public List<Seat> Seat { get; set; }
    }
}
