namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class ServicePlanMetricsModel
    {
        public string ServicePlanName { get; set; }
        public string ServicePlanFriendlyName { get; set; }
        public int cpuPercentageAverage { get; set; }
        public int cpuPercentageMax { get; set; }
        public int usedMemoryPercentageAverage { get; set; }
        public int usedMemoryPercentageMax { get; set; }

        public PieScoresModel ServicePlanCPUPie
        {
            get {
                return new PieScoresModel
                {
                     FirstScore = 100 - cpuPercentageMax,
                     SecondScore = cpuPercentageAverage,
                     ThirdScore = cpuPercentageMax,
                };
            }
        }

        public PieScoresModel ServicePlanMemoryPie
        {
            get
            {
                return new PieScoresModel
                {
                    FirstScore = 100 - usedMemoryPercentageMax,
                    SecondScore = usedMemoryPercentageAverage,
                    ThirdScore = usedMemoryPercentageMax,
                };
            }
        }

    }
}
