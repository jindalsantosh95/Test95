using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Mapper
{
    public class TicketVoidCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(VoidTicketDetails voidTicketRequest, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Data = voidTicketRequest,
                Type = EnumOperations.GetEnumDescription((OrderTicketEventType)Enum.Parse(typeof(OrderTicketEventType), OrderTicketEventType.OrderTicketDetail.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.TicketVoidEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
