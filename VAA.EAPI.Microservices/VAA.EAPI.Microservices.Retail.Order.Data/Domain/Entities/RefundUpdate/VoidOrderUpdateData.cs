using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.RefundUpdate
{
    public class VoidOrderUpdateData
    {
        public string OrderRef { get; set; }
        public List<FulfillmentDetails> FulfilmentId { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
