using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
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

        public static explicit operator OrderItems(JObject source)
        {
            return new()
            {
                ProductType = (ProductType)Enum.Parse(typeof(ProductType), source["ProductType"].ToString()),
                OrderItemId = source["OrderItemId"]?.ToString(),
                CustRefIds = source["CustRefIds"]?.Select(a => (string)a),
                SegmentRefIds = source["SegmentRefIds"]?.Select(a => (string)a),
                AccountableDocumentRefIds = source["AccountableDocumentRefIds"]?.Select(a => (string)a)
            };
        }
    }
}
