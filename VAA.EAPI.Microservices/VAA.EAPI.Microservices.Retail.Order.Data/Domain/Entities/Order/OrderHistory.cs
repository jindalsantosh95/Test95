using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order
{
    public class OrderHistory
    {
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
        public string Version { get; set; }
        public string ChangeDateTime { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public string OrderStatus { get; set; }
        public bool IsSplit { get; set; }
        public string ReasonCode { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
