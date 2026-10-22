


using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class Fulfillment
    {
        public IEnumerable<Ticket> Tickets { get; set; }
        public IEnumerable<Seat> Seat { get; set; }
    }
}
