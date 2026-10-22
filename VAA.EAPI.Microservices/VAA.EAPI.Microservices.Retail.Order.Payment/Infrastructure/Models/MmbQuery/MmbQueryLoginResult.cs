using System.Net;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery
{
    public sealed record MmbQueryLoginResult(HttpStatusCode StatusCode, MmbQueryLoginResponse? Body, string? CookieHeader);
}
