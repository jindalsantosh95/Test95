using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember
{
    public class DelinkAccountMemberPatchEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(DelinkAccountMemberPatchEvent patchEvent)
        {
            if (patchEvent == null) throw new ArgumentNullException(nameof(patchEvent));

            return new CloudEvent()
            {
                Data = patchEvent,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.DeleteAccountOnMemberUpdateEvent.ToString())),
                Source = "loyaltymembermanagementeventmessages",
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.DeleteAccountOnMemberUpdateEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
