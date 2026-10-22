using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatFulfilmentResponse
    {
        public string OrderNumber { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
        public string OrderId { get; set; }
    }
}
