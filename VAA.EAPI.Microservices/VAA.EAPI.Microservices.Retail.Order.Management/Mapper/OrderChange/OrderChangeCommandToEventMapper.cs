using System;
using System.Linq;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    class OrderChangeCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderChangeRequestData orderChangeRequest)
        {
            return new CloudEvent()
            {
                Data = orderChangeRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderChangeInitiate.ToString())),
                Source = orderChangeRequest.OrderAmendments.FirstOrDefault().ProductType.ToString() == Enum.GetName(OrderAmendment.ProductTypes.SeatEnum) ? orderChangeRequest.OrderAmendments.First().FlightItems.Any(x => x.ActionContext == FlightItems.ActionContexts.Delete) ? ConfigReader.Get(Constants.Environment.OrderChangeSeatDeleteServiceBusTopicName) : ConfigReader.Get(Constants.Environment.OrderChangeSeatAddServiceBusTopicName) : ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
