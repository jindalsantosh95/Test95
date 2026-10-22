using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout
{
    public sealed record CjsCheckoutPaymentAmountPayload(
        [property: JsonProperty("amountToPay")] decimal AmountToPay);
}

