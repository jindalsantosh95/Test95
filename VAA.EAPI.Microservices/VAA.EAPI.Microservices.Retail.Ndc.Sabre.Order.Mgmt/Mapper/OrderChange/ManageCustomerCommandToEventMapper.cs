using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class ManageCustomerCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(ManageCustomerCommand manageCustomerRequest)
        {
            return new CloudEvent()
            {
                Data = manageCustomerRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.ManageCustomerInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = (manageCustomerRequest.TicketedNameCorrection || manageCustomerRequest.UnTicketedNameCorrection) ? Constants.Application.ManageCustomerNameCorrectionEventSubject : manageCustomerRequest.AmendApis ? Constants.Application.ManageCustomerAmendApisEventSubject : Constants.Application.ManageCustomerEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent CommandToCloudEventMappper(OrderDocument orderDocument, string id)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = orderDocument,
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
            };
        }
    }
}
