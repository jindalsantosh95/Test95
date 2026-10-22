using Microsoft.EntityFrameworkCore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

[Keyless]
public partial class HealthCheckOfferPriceView
{
    public int RequestCount { get; set; }
    public string StatusCode { get; set; }
    public decimal Percentage { get; set; }
    public string StatusColor { get; set; }
}
