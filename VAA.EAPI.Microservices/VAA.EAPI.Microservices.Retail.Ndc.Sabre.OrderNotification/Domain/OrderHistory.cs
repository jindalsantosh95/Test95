using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    public class OrderHistory
    {
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
        public string Version { get; set; }
        public string ChangeDateTime { get; set; }
        public ChangeTypeEnum ChangeType { get; set; }
    }
}
