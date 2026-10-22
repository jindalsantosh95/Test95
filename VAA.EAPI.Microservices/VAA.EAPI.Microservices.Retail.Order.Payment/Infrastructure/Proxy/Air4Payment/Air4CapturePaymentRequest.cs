using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class Air4CapturePaymentRequest
    {
        public IEnumerable<FormsOfPayment> formsOfPayments { get; set; }
    }

}