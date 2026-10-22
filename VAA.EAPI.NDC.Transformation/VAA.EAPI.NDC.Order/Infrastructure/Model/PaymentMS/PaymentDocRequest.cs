using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class PaymentDocRequest
    {
        public string Type { get; set; }
        public ProductType?  ProductType { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
    }
}
