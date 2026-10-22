using System;

namespace VAA.EAPI.Microservices.Insights.Domain.ADO
{
    public class ADOBuildTimeline
    {
        public string parentId { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string result { get; set; }
        public DateTime? finishTime { get; set; }
        public int order { get; set; }

    }
}
