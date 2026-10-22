using Azure.Messaging;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.SSR;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.Remarks;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate
{
    public class OrderCreateCommandToEventMapper
    {
        public static  CloudEvent EventMapper(OrderCreateRequest requestData, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = requestData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderCreate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderCreateEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

        public static CloudEvent EventMapper(OrderDocument requestData, string id)
        {
            var cloudEvent = new CloudEvent
            {
                Id = id,
                Data = requestData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderStore.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderStoreEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion               
            };

            return cloudEvent;
        }

        public static CloudEvent EventMapper(CloudEventData requestData, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = requestData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.CustomerEnrollmentEventType.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerOrderEnrollmentServiceBusTopicName),
                Subject = Constants.Application.CustomerOrderEnrollmentEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion               
            };

            return cloudEvent;
        }

        public static CloudEvent EventMapper(OrderChange requestData, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = requestData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderSsr.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderAddSsrSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

        public static CloudEvent EventMapper(AddSpecialRemarkRequest requestData, string id)
        {
            return new CloudEvent
            {
                Id = id,
                Data = requestData,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.OrderSpecialRemarks.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderAddSpecialRemarksSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
