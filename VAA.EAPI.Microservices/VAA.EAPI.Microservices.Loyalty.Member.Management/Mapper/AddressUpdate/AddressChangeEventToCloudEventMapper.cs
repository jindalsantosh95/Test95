using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.AddressUpdate
{
    public class AddressChangeEventToCloudEventMapper
    {
        public static CloudEvent CustomerSearchDocumentUpdateCommandToEventMapper(CustomerSearchUpdateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateCustomerSearchDocEvent.ToString())),
                Source = Constants.Application.CustomerServiceBusTopicName,
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateCustomerSearchDocEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent CustomerDocumentUpdateCommandToEventMapper(CustomerUpdateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateCustomerDocEvent.ToString())),
                Source = Constants.Application.CustomerServiceBusTopicName,
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.UpdateCustomerDocEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent AddressHistoryCreateCommandToEventMapper(AddressHistoryCreateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.CreateAddressHistoryDocEvent.ToString())),
                Source = Constants.Application.CustomerServiceBusTopicName,
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.CreateAddressHistoryDocEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
