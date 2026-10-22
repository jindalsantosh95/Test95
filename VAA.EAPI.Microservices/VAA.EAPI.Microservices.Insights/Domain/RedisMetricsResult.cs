using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public class RedisMetricsResult
    {
        public string RedisCacheName { get; set; }
        public int NumberOfConnectedClients { get; set; }
        public int UsedMemory { get; set; }
        public int UsedMemoryPercentage { get; set; }
    }
}
