using System.Net;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout
{
    // NOTE: SetCookieHeaders contains cookie-pairs formatted for the request "Cookie" header (e.g. "a=1; b=2").
    // The application layer splits these pairs into multiple "Set-Cookie" response headers.
    public sealed record CjsCheckoutPaymentAppResult(
        HttpStatusCode StatusCode,
        CjsCheckoutPaymentAppResponse? Body,
        string? SetCookieHeaders = null);
}
