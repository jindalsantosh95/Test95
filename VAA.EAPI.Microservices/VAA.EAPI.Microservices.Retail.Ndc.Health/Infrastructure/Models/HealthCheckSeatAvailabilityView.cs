using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

[Keyless]
public partial class HealthCheckSeatAvailabilityView
{
    public int RequestCount { get; set; }
    public string IsPrime { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string StatusCode { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Percentage { get; set; }
}
