using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SeatFulfilmentResponse
    {
        public string OrderNumber { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<Error> Error { get; set; }
        public string OrderId { get; set; }
    }
}
