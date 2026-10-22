using Microsoft.EntityFrameworkCore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models
{
    [Keyless]
    public class HealthCheckOrderReshopView
    {
        public int RequestCount { get; set; }
        public string StatusCode { get; set; }
        public decimal Percentage { get; set; }
        public string ReshopRequest { get; set; }
        public string CancelReshopRequest { get; set; }
        public string NameCorrectionReshopRequest { get; set; }
    }
}
