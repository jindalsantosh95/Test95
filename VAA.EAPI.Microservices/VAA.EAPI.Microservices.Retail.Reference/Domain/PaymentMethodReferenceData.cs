using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class PaymentMethodReferenceData
    {
        public IEnumerable<PaymentMethod> PaymentMethodDetails { get; set; }
    }
}
