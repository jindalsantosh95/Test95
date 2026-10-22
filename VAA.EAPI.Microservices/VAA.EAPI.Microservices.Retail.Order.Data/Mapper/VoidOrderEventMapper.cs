using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class VoidOrderEventMapper
    {
        public static CloudEvent OrderDetailMapper(VoidOrderDetails voidOrderDetails, OrderReadRequestQuery request)
        {
            return new CloudEvent
            {
                Id = request.Id,
                Subject = Constants.Application.OrderReadResponseEventSubject,
                Type = EnumOperations.GetEnumDescription((EventType)Enum.Parse(typeof(EventType), EventType.OrderReadEventType.ToString())),
                Data = voidOrderDetails,
                Source = ConfigReader.Get(Constants.Application.ServiceBusTopicNameTicket),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
