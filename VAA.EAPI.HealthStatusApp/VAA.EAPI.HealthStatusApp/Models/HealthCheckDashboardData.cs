using System;
using System.Collections.Generic;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;
namespace VAA.EAPI.HealthStatusApp.Models
{

    public class HealthCheckDashboardData
    {
        public int APIMScore { get; set; }
        public int GraphQlScore { get; set; }
        public int ServiceBusScore { get; set; }
        public int CosmosScore { get; set; }
        public int UnhealthyGraphQlScore { get; set; }
        public int UnhealthyServiceBusScore { get; set; }
        public int UnhealthyCosmosScore { get; set; }

        public int UnhealthyScore { get; set; }


        public Dictionary<string, bool> GraphQlServices { get; set; }
        public Dictionary<string, bool> ServiceBusServices { get; set; }
        public Dictionary<string, bool> CosmosServices { get; set; }
        public string? IssueAlert { get; set; }
        public IEnumerable<DashboardIssue> Issues { get; set; }
    }

}
