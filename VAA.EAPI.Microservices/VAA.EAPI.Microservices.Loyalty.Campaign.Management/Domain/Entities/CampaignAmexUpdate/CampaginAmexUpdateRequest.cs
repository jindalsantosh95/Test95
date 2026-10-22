using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate
{
    public class  CampaginAmexUpdateRequest
    {
        public string AmexUniqueId { get; set; }
        public string FCnumber { get; set; }        
    }
}
