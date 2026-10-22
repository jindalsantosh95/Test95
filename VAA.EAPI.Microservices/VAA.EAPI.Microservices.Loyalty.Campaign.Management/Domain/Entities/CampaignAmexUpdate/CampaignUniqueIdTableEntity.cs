using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate
{
    public class CampaignUniqueIdTableEntity : TableEntity
    {
        public string UniqueId { get; set; }
        public string FCNumber { get; set; }
        public string UsedTimestamp { get; set; }
    }
}
