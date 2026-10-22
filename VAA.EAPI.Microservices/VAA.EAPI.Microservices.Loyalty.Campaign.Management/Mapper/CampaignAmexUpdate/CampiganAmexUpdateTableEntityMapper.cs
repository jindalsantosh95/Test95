using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAmexUpdate
{
    public class CampiganAmexUpdateTableEntityMapper
    {
        public static CampaignUniqueIdTableEntity UpdateCampaignUniqueIdTableEntities(CampaginAmexUpdateRequest campaginAmexUpdateRequest)
        {
            return new CampaignUniqueIdTableEntity()
            {                
                PartitionKey = Constants.Application.CampaignAmexPartitionKey,
                RowKey = campaginAmexUpdateRequest.AmexUniqueId.ToUpperInvariant(),
                UniqueId = campaginAmexUpdateRequest.AmexUniqueId,
                FCNumber = campaginAmexUpdateRequest.FCnumber,
                UsedTimestamp = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };
        }
    }
}
