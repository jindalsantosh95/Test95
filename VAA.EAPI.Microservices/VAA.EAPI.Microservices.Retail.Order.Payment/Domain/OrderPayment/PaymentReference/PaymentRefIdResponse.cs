using System;
using System.ServiceModel;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class PaymentRefIdResponse
    {
        public static explicit operator PaymentRefIdResponse(UriBuilder source) => new() { HPPLink = (source ?? throw new ArgumentNullException(nameof(source))).ToString()};
        public static explicit operator PaymentRefIdResponse(string source) => new() { HPPLink = source ?? throw new ArgumentNullException(nameof(source)) };

        public string HPPLink { get; set; }
        public string? PaymentURL { get; set; }
    }
}
