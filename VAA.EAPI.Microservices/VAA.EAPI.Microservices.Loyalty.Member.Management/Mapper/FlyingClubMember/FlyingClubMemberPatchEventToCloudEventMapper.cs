using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember
{
    public class FlyingClubMemberPatchEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(FlyingClubMemberPatchEvent patchEvent)
        {
            if (patchEvent == null) throw new ArgumentNullException(nameof(patchEvent));
            return new CloudEvent()
            {
                Data = patchEvent,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateFlyingClubMemberEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTransactionTopicName),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateFlyingClubMemberEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
