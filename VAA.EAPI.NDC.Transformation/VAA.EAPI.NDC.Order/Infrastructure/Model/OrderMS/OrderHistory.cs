using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
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
        public IEnumerable<TimeLimit> TimeLimits { get; set; }

    }
}
