using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData
{
    public class OrderDocument
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
    }
}
