using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models
{
    [Keyless]
    public class HealthCheckOrderListView
    {
        public int RequestCount { get; set; }
        public string StatusCode { get; set; }
        public decimal Percentage { get; set; }
        public string StatusColor { get; set; }
    }
}
