using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class CancelTransactionEventMapper
    {
        public static CloudEvent CancelTransactionCloudEventMapper(CancelTransactionRequest eventData, string correlationId)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Id = correlationId,
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())),
                Source = Constants.Application.ServiceBusTopicNameForVred,
                Subject = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
