using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class OrderItems
    {
        public ProductType ProductType { get; set; }
        public string OrderItemId { get; set; }
        public IEnumerable<string> CustRefIds { get; set; }
        public IEnumerable<string> SegmentRefIds { get; set; }
        public IEnumerable<string> AccountableDocumentRefIds { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<OrderService> Services { get; set; }
    }
}
