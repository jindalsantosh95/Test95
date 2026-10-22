using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class PaymentAuthorizeRequest
    {
        public IEnumerable<Order> Orders { get; set; }
        public BasicInformation BasicInformation { get; set; }
        public IEnumerable<FormOfPayment> FormOfPayment { get; set; }
        public string TrackID { get; set; }
        public bool FullRefund { get; set; }
    }
}

