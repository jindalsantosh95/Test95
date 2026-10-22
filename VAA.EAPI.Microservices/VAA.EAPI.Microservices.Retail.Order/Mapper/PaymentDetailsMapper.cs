using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper
{
    public class PaymentDetailsMapper
    {
        public static CloudEvent ClearPaymentDetails(OrderCreateRequest orderCreateRequest, string paymentReferenceId)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = new PaymentRequest()
                {
                    PaymentReferenceId = paymentReferenceId
                },
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.ClearPaymentDetails.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicClearPaymentDetails),
                Subject = Constants.Application.ClearPaymentDetailsEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }
    }
}