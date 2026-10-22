using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class Offer
    {
        public string ProductType { get; set; }
        public string ProductCode { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
    }
}
