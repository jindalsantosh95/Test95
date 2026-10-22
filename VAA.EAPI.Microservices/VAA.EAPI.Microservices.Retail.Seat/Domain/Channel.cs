using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain
{
    public class Channel : TableEntity
    {
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string DlApplicationName { get; set; }
        public string DlApplicationId { get; set; }
        public string DlAgentId { get; set; }
        public string DlPoolType { get; set; }
        public string DlRoleIndicator { get; set; }
    }
}
