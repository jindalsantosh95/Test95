namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class RedisCacheMetricsModel
    {
        public string RedisCacheName { get; set; }
        public int NumberOfConnectedClients { get; set; }
        public int UsedMemory { get; set; }
        public int UsedMemoryInMB
        {
            get
            {
                return (int)(UsedMemory / 1048576);
            }
        }

        public int UsedMemoryPercentage { get; set; }
        public int AvailableMemoryInMB
        {
            get
            {
                var availablePercentage = 100 - UsedMemoryPercentage;
                if (UsedMemoryPercentage == 0 && UsedMemoryInMB > 0) return 806 -UsedMemoryInMB ;
                var i = availablePercentage / UsedMemoryPercentage;
                return UsedMemoryInMB * i;
            }
        }
        public PieScoresModel RedisPie
        {
            get {
                return new PieScoresModel
                {
                     FirstScore = AvailableMemoryInMB,
                     SecondScore = UsedMemoryInMB
                };
            }
        }

    }
}
