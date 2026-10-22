namespace VAA.EAPI.Infrastructure.PSS.Models
{
    public class Air4RequestContext
    {

        public string AppId { get; set; }
        public string AppChannelName { get; set; }
        public string TestLab { get; set; }
        public string DebugInfo { get; set; }
        public string Location { get; set; }
        public string PoolType { get; set; }
        public string TimeoutInactivity { get; set; }
        public string TimeoutSession { get; set; }
    }
}
