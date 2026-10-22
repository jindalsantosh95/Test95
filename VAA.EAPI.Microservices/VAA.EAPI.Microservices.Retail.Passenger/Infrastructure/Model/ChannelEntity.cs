using Microsoft.Azure.Cosmos.Table;


namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model
{

    public class ChannelEntity : TableEntity
    {
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string DlApplicationName { get; set; }
        public string DlApplicationId { get; set; }
    }
}
