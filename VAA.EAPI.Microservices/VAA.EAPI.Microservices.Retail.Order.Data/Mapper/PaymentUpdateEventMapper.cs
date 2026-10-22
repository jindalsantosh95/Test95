using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class PaymentUpdateEventMapper
    {
        public static CloudEvent EventMapper(PaymentUpdateStatus status, PaymentUpdateCommand payment, PaymentUpdateRequestData requestData)
        {
            return new CloudEvent()
            {
                Id = payment.Id,
                Source = payment.Source,
                Subject = Constants.Application.OrderPaymentStatusUpdateSubject,
                DataContentType = payment.DataContentType,
                Time = DateTime.UtcNow.ToString(),
                Specversion = payment.Specversion,
                Type = EnumOperations.GetEnumDescription((EventType)Enum.Parse(typeof(EventType), EventType.OrderPaymentUpdateStatus.ToString())),
                Data = new PaymentUpdateEventData()
                {
                    OrderReference = requestData.OrderReference,
                    Status = status.ToString(),
                    PayLoad = null
                }
            };
        }
    }
}
