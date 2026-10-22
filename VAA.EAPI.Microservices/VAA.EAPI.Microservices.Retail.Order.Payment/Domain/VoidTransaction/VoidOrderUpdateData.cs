using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.VoidTransaction
{
    public class VoidOrderUpdateData
    {
        public string OrderRef { get; set; }
        public List<FulfilmentId> FulfilmentId { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
