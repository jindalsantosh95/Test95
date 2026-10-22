namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class DeploymentMetricsModel
    {
        public string ServiceName { get; set; }
        public DateTime LastDeployment { get; set; }
        public string DaysSinceDeployment
        {
            get
            {
                return (DateTime.Now.Date - LastDeployment.Date).TotalDays.ToString();
            }
        }
    }
}
