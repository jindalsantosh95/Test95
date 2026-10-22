using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister
{
    public class CampaignRegisterEventMapper
    {
        public static CloudEvent CommandToTragetedMembershipEventMapper(CampaignDetails campaignData)
        {
            return new CloudEvent()
            {
                Data = campaignData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipUpdateEventInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateTargetedMembership.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
        public static CloudEvent CommandToCampaignRegisterEventMapper(CampaignRegisterEventData eventData)
        {
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignRegisterInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignRegisterEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
        public static CloudEvent CommandToCampaignEnrolmentDocEventMapper(CampaignEnrolmentDocDetails campaignEnrolmentDoc)
        {
            return new CloudEvent()
            {
                Data = campaignEnrolmentDoc,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.CampaignEnrolmentDocEventInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CampaignEnrolmentDocEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
        public static CloudEvent CommandToMemberProfileUpdateEventMapper(MemberProfileUpdateEventData memberProfileUpdateData)
        {
            return new CloudEvent()
            {
                Data = memberProfileUpdateData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.MemberProfileUpdateEventInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateFlyingClubMember.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}