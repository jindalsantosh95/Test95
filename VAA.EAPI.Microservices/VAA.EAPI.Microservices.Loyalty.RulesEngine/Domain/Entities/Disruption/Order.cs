using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption
{
    public class Order
    {
        public DataList DataList { get; set; }

        public Pos PointOfSale { get; set; }

        public UtcAndLocalDateTime CreationDateTime { get; set; }

        public OrderNumber OrderNumber { get; set; }

        public BookingReference BookingReference { get; set; }

        public IEnumerable<OrderItems> OrderItems { get; set; }

        public Parties Parties { get; set; }

        public string ChangeType { get; set; }

        public UtcAndLocalDateTime ChangeDateTime { get; set; }
    }
}
