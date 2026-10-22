using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.VoidTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.VoidTransaction
{
    public class VoidTransactionMapper
    {
        public static CloudEvent MapToDataEvent(VoidTransactionEventCommand voidTransactionEventCommand, VoidOrderUpdateData voidOrderUpdateData)
        {
            return new CloudEvent()
            {
                Id = voidTransactionEventCommand.Id,
                Data = voidOrderUpdateData,
                Type = EnumOperations.GetEnumDescription
                       (
                        value: (PaymentEventType)Enum
                                .Parse
                                (
                                    enumType: typeof(PaymentEventType),
                                    value: PaymentEventType.VoidOrderUpdateInitiate.ToString()
                                )
                       ),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.VoidTransactionEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
