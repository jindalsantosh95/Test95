using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignPromotionLinking
{
    public class CampaignPromotionLinkCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(CampaignPromotionLinkRequestData campaignPromotionLinkRequestData)
        {
            return new CloudEvent()
            {
                Data = campaignPromotionLinkRequestData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.LinkCampaignPromotion.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.LinkCampaignPromotion.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
