using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Domain.Enums;

namespace VAA.EAPI.Microservices.CRM.Mapper
{
    public class UpdatePaymentReqeustCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(UpdatePaymentReqeustCommand orderChangeRequest)
        {
            return new CloudEvent()
            {
                Data = orderChangeRequest,
                Type = EnumOperations.GetEnumDescription((CRMPaymentEventTypes)Enum.Parse(typeof(CRMPaymentEventTypes), CRMPaymentEventTypes.CRMPaymentUpdate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CRMPaymentEventHandlerTopicName),
                Subject = Constants.Application.CRMPaymentUpdateEventSubject,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
