using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Insights.Models
{
    public class ADODeploymentCollection
    {
        public DateTime LastUpdated { get; set; }
        public IEnumerable<ADODeploymentStatus> DeploymentStatuses { get; set; } = Enumerable.Empty<ADODeploymentStatus>();
    }
}
