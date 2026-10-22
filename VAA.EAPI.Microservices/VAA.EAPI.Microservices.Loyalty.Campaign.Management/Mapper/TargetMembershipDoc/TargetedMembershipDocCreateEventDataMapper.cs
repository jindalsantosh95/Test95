using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.TargetMembershipDoc
{
    public class TargetedMembershipDocCreateEventDataMapper
    {
        public static TargetedMembershipDocCreateEventData TargetMembershipDocCreateEventDataMapper(CampaignAssignEventData assignEventData, CampaignDetails campaignData)
        {
            if (assignEventData == null) throw new ArgumentNullException(nameof(assignEventData));
            return new TargetedMembershipDocCreateEventData
            {
                CustomerId = assignEventData.CustomerId,
                TargetedMembershipDocument = new TargetedMembershipDocument
                {
                    AssignmentDate = (assignEventData.DocumentStatus == Constants.Application.EnrolDocStatus) ? DateTime.UtcNow : null,
                    Comments = Constants.Application.CreateTargetedMembershipComment,
                    CompletedDate = string.IsNullOrEmpty(campaignData.CampaignEndDate?.ToString()) ? DateTime.UtcNow.AddMonths(6) : campaignData.CampaignEndDate,
                    ContactedDate = DateTime.UtcNow,
                    ProfilingCampaignSourceId = assignEventData.CampaignId,
                    StatusCode = assignEventData.DocumentStatus,
                    createdBy = assignEventData.CreatedBy,
                }
            };
        }
        public static CloudEvent CommandToTragetedMembershipEventMapper(TargetedMembershipDocCreateEventData campaignData)
        {
            return new CloudEvent()
            {
                Data = campaignData,
                Type = EnumOperations.GetEnumDescription((CampaignEventType)Enum.Parse(typeof(CampaignEventType), CampaignEventType.TargetedMembershipCreateEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTranscationTopicName),
                Subject = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreateTargetedMembership.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
