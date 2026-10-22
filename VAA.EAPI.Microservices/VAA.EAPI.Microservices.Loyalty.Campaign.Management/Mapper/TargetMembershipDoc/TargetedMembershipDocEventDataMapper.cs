using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.TargetMembershipDoc
{
    public class TargetedMembershipDocEventMapper
    {
        public static TargetedMembershipDocUpdateEventData TargetedMembershipDocUpdateEventDataMapper(CampaignRegisterEventData campaignRegisterEventData)
        {
            if (campaignRegisterEventData == null) throw new ArgumentNullException(nameof(campaignRegisterEventData));
            return new TargetedMembershipDocUpdateEventData
            {
                CustomerId = campaignRegisterEventData.CustomerId,
                TargetedMembershipDocument = new TargetedMembershipUpdateDocument()
                {
                    Id = campaignRegisterEventData.DocumentId,
                    AssignmentDate = DateTime.UtcNow,
                    Comments = Constants.Application.UpdateTargetedMembershipComment,
                    StatusCode = campaignRegisterEventData.DocumentStatus,
                    CompletedDate = string.IsNullOrEmpty(campaignRegisterEventData.CampaignEndDate?.ToString()) ? DateTime.UtcNow.AddMonths(6).ToString() : campaignRegisterEventData.CampaignEndDate?.ToString(),
                    ProfilingCampaignSourceId = campaignRegisterEventData.CampaignId,
                    CreatedBy = campaignRegisterEventData.CreatedBy
                }
            };
        }
        public static CloudEvent TargetedMembershipDocUpdateEventMapper(TargetedMembershipDocUpdateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipUpdateEventInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTranscationTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateTargetedMembership.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
