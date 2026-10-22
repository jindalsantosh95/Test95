using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class OrderNotificationEventMapper
    {
        public static CloudEvent OrderDetailMapper(OrderCreateRequestCommand request, OrderCreateResponse response)
        {
            return new CloudEvent
            {
                Id = request.Id,
                Subject = Constants.Application.OrderCreateResponseEventSubject,
                Type = EnumOperations.GetEnumDescription((EventType)Enum.Parse(typeof(EventType), EventType.OrderNotificationCreateStatusEventType.ToString())),
                Data = response,
                Source = ConfigReader.Get(Constants.Application.ServiceBusTopicName),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
