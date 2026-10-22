using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.VoidTransaction
{
    public class CPDBusinessErrorMapper
    {
        public static CloudEvent MapToEmailEvent(VoidTransactionEventCommand voidTransactionEventCommand)
        {
            return new CloudEvent()
            {
                Id = voidTransactionEventCommand.Id,
                Data = null,// Yet to be finalized
                Type = EnumOperations.GetEnumDescription
                       (
                        value: (PaymentEventType)Enum
                                .Parse
                                (
                                    enumType: typeof(PaymentEventType),
                                    value: PaymentEventType.CPDErrorCommunicationInitiate.ToString()
                                )
                       ),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.CPDBusinessErrorEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
