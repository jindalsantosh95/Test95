using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.CustomerSearch
{
    public static class CustomerProfileEventToCloudEventMapper
    {
        public static CloudEvent EventToCloudEventMapper(CustomerProfileAuditEvent eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((CustomerEventType)Enum.Parse(typeof(CustomerEventType), CustomerEventType.CreateCustomerProfile.ToString())),
                Source = ConfigReader.Get(Constants.Environment.AuditServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((CustomerEventType)Enum.Parse(typeof(CustomerEventType), CustomerEventType.CreateCustomerProfile.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
