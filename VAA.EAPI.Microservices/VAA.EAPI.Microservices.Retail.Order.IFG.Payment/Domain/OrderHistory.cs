using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    public class OrderHistory
    {
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public DataList DataList { get; set; }
    }
}
