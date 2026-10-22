using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Mapper
{
    public class OrderRefundEventMapper
    {
        public static CloudEvent MapToOrderRefundEvent(VoidOrderDetailEventProcessCommand voidOrderDetailEventProcessCommand)
        {
            return new CloudEvent()
            {
                Id = voidOrderDetailEventProcessCommand.Id,
                Data = voidOrderDetailEventProcessCommand.Data,
                Type = EnumOperations.GetEnumDescription
                       (
                        value: (OrderTicketEventType)Enum
                                .Parse
                                (
                                    enumType: typeof(OrderTicketEventType),
                                    value: OrderTicketEventType.VoidTransactionInitiate.ToString()
                                )
                       ),
                Source = ConfigReader.Get(Constants.Environment.OrderPaymentServiceBusTopicName),
                Subject = Constants.Application.VoidTransactionEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
