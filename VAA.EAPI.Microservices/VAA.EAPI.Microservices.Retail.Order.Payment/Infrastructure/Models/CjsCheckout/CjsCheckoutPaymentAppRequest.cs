using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout
{
    public sealed record CjsCheckoutPaymentAppRequest(
        [property: JsonProperty("bookingReference")] string BookingReference,
        [property: JsonProperty("surname")] string Surname,
        [property: JsonProperty("dateOfBirth")] string DateOfBirth,
        [property: JsonProperty("returnUrl")] string ReturnUrl,
        [property: JsonProperty("payPalClientUrl")] string PayPalClientUrl,
        [property: JsonProperty("expiryUrl")] string ExpiryUrl,
        [property: JsonProperty("contextPath")] string ContextPath,
        [property: JsonProperty("amountToPay")] decimal? AmountToPay);
}

