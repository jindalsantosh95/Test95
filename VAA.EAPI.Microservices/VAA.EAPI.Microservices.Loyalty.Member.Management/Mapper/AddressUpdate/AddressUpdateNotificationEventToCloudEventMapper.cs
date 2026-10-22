using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.AddressUpdate
{
    public class AddressUpdateNotificationEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(AddressChangeEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.AddressChangeEvent.ToString())),
                Source = Constants.Application.ServiceBusTopicName,
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.AddressChangeEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
