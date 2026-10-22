using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUpdate
{
    public class CampaignUpdateCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(CampaignUpdateRequestData campaignDataRequest)
        {
            return new CloudEvent()
            {
                Data = campaignDataRequest,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignManagementUpdateCampaign.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateCampaign.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
