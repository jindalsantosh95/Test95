using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Mapper
{
    public class CampaignDetailMapper
    {
        public static CampaignDetails MapCamppaignData(Models.Campaign campaignData)
        {
            if (campaignData is null) throw new ArgumentNullException(nameof(campaignData));

            return new CampaignDetails
            {
                CampaignCode = campaignData.CampaignCode,
                CampaignStartDate = campaignData.StartDate,
                CampaignEndDate = campaignData.EndDate,
                CampaignId = campaignData.RowId,
                CampaignType = campaignData.Type,
                RegistrationRequired = campaignData.RegistrationRequired,
                CampaignName = campaignData.Name,
                CampaignStatus = campaignData.Status,
                CampaignDescription = campaignData.Description,
            };
        }
    }
}
