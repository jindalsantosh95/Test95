using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout
{
    public sealed record CjsCheckoutPaymentAppPayload(
        [property: JsonProperty("returnUrl")] string ReturnUrl,
        [property: JsonProperty("payPalClientUrl")] string PayPalClientUrl,
        [property: JsonProperty("expiryUrl")] string ExpiryUrl,
        [property: JsonProperty("contextPath")] string ContextPath);
}

