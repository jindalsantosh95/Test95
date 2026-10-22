using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Mapper
{
    public class MetricsMapper
    {
        public static RedisMetricsResult MapRedisMetricsResult(MetricsResponse metrics, string name)
        {
            var result = new RedisMetricsResult { RedisCacheName = name };
            if (metrics?.Value == null) return result;

            foreach (var metric in metrics.Value)
            {
                var latestData = metric.Timeseries?.FirstOrDefault()?.Data?.LastOrDefault();
                if (latestData == null) continue;

                switch (metric.Name?.Value?.ToLowerInvariant())
                {
                    case "connectedclients":
                        result.NumberOfConnectedClients = (int)(latestData.Average);
                        break;
                    case "allusedmemory":
                        result.UsedMemory = (int)(latestData.Average);
                        break;
                    case "allusedmemorypercentage":
                        result.UsedMemoryPercentage = (int)(latestData.Average);
                        break;
                }
            }

            return result;
        }

        public static ServiceBusMetricsResult MapServiceBusMetricsResult(MetricsResponse metrics, string name)
        {
            var result = new ServiceBusMetricsResult { ServiceBusName = name };
            if (metrics?.Value == null) return result;

            foreach (var metric in metrics.Value)
            {
                var latestData = metric.Timeseries?.FirstOrDefault()?.Data?.LastOrDefault();
                if (latestData == null) continue;

                switch (metric.Name?.Value?.ToLowerInvariant())
                {
                    case "deadletteredmessages":
                        result.NumberOfDeadLetterMessages = (int)(latestData.Average);
                        break;
                    case "activemessages":
                        result.NumberOfMessagesInQueue = (int)(latestData.Average);
                        break;
                }
            }

            return result;
        }

        public static CosmosDbMetricResult MapCosmosDbMetricsResult(MetricsResponse metrics, string name)
        {
            var internalList = new List<CosmosDbMetricsData>();

            if (metrics?.Value != null)
            {
                foreach (var metric in metrics.Value)
                {
                    if (metric.Name?.Value?.ToLowerInvariant() == "normalizedruconsumption")
                    {
                        foreach (var dataPoint in metric.Timeseries?.SelectMany(ts => ts.Data))
                        {
                            internalList.Add(new CosmosDbMetricsData
                            {
                                TimeStamp = dataPoint.TimeStamp,
                                MaxNormalizedRUConsumptionPercentage = (int)(dataPoint.Maximum)
                            });
                        }
                    }
                }
            }

            return new CosmosDbMetricResult
            {
                ServiceName = name,
                CosmosDbMetrics = internalList.AsEnumerable()
            };
        }



        public static ServicePlanMetricResult MapServicePlanMetricsResult(MetricsResponse metrics, string name)
        {
            var result = new ServicePlanMetricResult { ServicePlanName = name };
            if (metrics?.Value == null) return result;

            foreach (var metric in metrics.Value)
            {
                var dataPoints = metric.Timeseries?.SelectMany(ts => ts.Data)?.ToList();
                if (dataPoints == null || !dataPoints.Any()) continue;

                switch (metric.Name?.Value?.ToLowerInvariant())
                {
                    case "cpupercentage":
                        result.CPUPercentageAverage = (int)dataPoints.Average(dp => dp.Average);
                        result.CPUPercentageMax = (int)dataPoints.Max(dp => dp.Maximum);
                        break;
                    case "memorypercentage":
                        result.UsedMemoryPercentageAverage = (int)dataPoints.Average(dp => dp.Average);
                        result.UsedMemoryPercentageMax = (int)dataPoints.Max(dp => dp.Maximum);
                        break;
                }
            }

            return result;
        }
    }
}
