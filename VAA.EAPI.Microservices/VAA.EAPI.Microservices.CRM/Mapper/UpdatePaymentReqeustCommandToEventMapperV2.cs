using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Domain.Enums;

namespace VAA.EAPI.Microservices.CRM.Mapper
{
    public class UpdatePaymentReqeustCommandToEventMapperV2
    {
        public static CloudEvent CommandToEventMapper(UpdatePaymentReqeustCommandV2 orderChangeRequest)
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
