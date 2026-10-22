using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class OrderChangeCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderChangeRequestData orderChangeRequest)
        {
            return new CloudEvent()
            {
                Data = orderChangeRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderChangeInitiate.ToString())),
                Source = orderChangeRequest.OrderAmendments?.First().ProductType.ToString() == Enum.GetName(OrderAmendment.ProductTypes.SeatEnum) ? ConfigReader.Get(Constants.Environment.OrderChangeSeatAddServiceBusTopicName) : ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent EventMapper(OrderDocument request, string id)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };
        }
    }
}
