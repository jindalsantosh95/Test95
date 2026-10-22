using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

[Keyless]
public partial class HealthCheckOrderCreateView
{
    public int RequestCount { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string HoldBooking { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string InstantBooking { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string Seat { get; set; }

    [StringLength(256)]
    [Unicode(false)]
    public string StatusCode { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Percentage { get; set; }
}
