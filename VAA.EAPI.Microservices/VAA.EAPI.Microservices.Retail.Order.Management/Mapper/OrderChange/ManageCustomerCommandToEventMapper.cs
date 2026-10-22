using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class ManageCustomerCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(ManageCustomerCommand manageCustomerRequest)
        {
            return new CloudEvent()
            {
                Data = manageCustomerRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.ManageCustomerInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.ManageCustomerEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
