using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class OrderHistory
    {
        public DataList DataList { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string Version { get; set; }
        public string ChangeDateTime { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public bool IsSplit { get; set; }
    }
}