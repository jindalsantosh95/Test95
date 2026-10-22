using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.StampFCNumToPNR
{
    public class FCNumberToPNRCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderChangeRequestData eventData, string correlationId)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Id = correlationId,
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.ManageSpecialServiceInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.ManageSpecialServiceEventSubject,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
