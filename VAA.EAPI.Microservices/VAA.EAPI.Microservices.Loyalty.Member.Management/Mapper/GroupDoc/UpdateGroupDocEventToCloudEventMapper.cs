using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc
{
    public class UpdateGroupDocEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(HouseHoldGroupUpdateEvent eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateGroupDocEvent.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTransactionTopicName),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateGroupDocEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
