using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCSeatUpdate
{
    public class SeatFulfilmentResponse
    {
        public string OrderNumber { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderId { get; set; }
        public bool Deletion { get; set; }
    }
}
