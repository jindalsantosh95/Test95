using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate
{
    public class CampaignAmexUpdateEventRequest
    {
        public string FCNumber { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
