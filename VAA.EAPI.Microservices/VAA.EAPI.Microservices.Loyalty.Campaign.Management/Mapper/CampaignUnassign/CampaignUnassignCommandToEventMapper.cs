using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUnassign
{
    public class CampaignUnassignCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(TargetedMembershipDocDeleteEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTranscationTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent CommandToEventMapperToDeleteFromReportingDb(TargetedMembershipDocDeleteEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.LoyaltyMemberServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipDeleteEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}