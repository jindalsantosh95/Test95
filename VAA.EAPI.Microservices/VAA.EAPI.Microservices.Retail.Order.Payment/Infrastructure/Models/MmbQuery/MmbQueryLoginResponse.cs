using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery
{
    public sealed record MmbQueryLoginResponse([property: JsonProperty("refreshToken")] string RefreshToken);
}

