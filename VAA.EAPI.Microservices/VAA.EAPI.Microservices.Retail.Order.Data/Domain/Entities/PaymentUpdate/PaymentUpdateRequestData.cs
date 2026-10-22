using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate
{
    public class PaymentUpdateRequestData
    {
        public string Channel { get; set; }
        public string OrderReference { get; set; }
        public List<OrderPaymentRequest> PaymentRequest { get; set; }
    }
}

