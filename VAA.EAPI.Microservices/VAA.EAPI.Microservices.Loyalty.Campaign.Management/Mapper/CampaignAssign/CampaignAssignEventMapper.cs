using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign
{
    public class CampaignAssignEventMapper
    {
        public static CloudEvent CommandToAssignEventMapper(CampaignAssignEventData eventData)
        {
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignAssignInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignAssignEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}