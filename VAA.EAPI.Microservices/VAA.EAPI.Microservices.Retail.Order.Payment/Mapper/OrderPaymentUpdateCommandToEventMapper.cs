using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper
{
    public class OrderPaymentUpdateCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderPaymentEventCommand orderPaymentRequest)
        {
            return new CloudEvent()
            {
                Data = orderPaymentRequest,
                Type = EnumOperations.GetEnumDescription((PaymentEventType)Enum.Parse(typeof(PaymentEventType), PaymentEventType.OrderPaymentUpdateInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderPaymentUpdateEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
