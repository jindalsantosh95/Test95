using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS
{
    public class OrderHistory
    {
        public DataList DataList { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string Version { get;set; }
        public string ChangeDateTime { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public bool IsSplit { get; set; }
        public string ReasonCode { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
