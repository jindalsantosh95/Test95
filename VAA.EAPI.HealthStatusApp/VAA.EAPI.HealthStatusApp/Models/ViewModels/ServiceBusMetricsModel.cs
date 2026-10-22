namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class ServiceBusMetricsModel
    {

        public string ServiceBusName { get; set; }
        public int NumberOfMessagesInQueue { get; set; }
        public int NumberOfDeadLetterMessages { get; set; }
    }
}
