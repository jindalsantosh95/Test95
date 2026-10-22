using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc
{
    public class HouseHoldAccountEmailTriggeringEventGridMapper
    {
        public static CloudEvent EventDataMapper(HouseHoldAccountEmailTriggeringEventRequest eventdata)
        {
            if (eventdata == null) throw new ArgumentNullException(nameof(eventdata));

            return new CloudEvent
            {
                Id = eventdata.Id,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                Data = eventdata.Data,
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.BasicEmailEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.EmailTopicEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
