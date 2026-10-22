using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{

    public class PaymentPurchaseAdvisoryMessage
    {
        public IEnumerable<Remark> Remarks { get; set; }
    }

}