using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Mapper
{
    public class OrderNotificationCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderNotificationRequestCommand orderNotificationRequestCommand)
        {
            return new CloudEvent()
            {
                Data = orderNotificationRequestCommand,
                Type = EnumOperations.GetEnumDescription
                       (
                        value: (OrderNotificationTypes)Enum
                                .Parse
                                (
                                    enumType: typeof(OrderNotificationTypes),
                                    value: OrderNotificationTypes.OrderNotificationInitiate.ToString()
                                )
                       ),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderNotificationEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
