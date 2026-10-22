using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class CreatePaymentDocCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(PaymentDocRequest splitPnrRequest, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = splitPnrRequest,
                Type = Constants.Application.CreatePaymentDocumentEventType,
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.CreatePaymentDocumentEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

    }
}
