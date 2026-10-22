using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain
{
    public class Channel : TableEntity
    {
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationId { get; set; }
        public string AgentId { get; set; }
        public string PoolType { get; set; }
        public string RoleIndicator { get; set; }
    }
}
