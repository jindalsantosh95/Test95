using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Entities
{
    public class CampaignUniqueIdTableEntity : TableEntity
    {
        public string UniqueId { get; set; }
        public string FCNumber { get; set; }
        public string UsedTimestamp { get; set; }
        
    }
}

