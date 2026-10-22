using Newtonsoft.Json.Linq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class TransactionMapper
    {
        public static TransactionCreateEvent MapTxnCreateRequestToCloudEvent(TransactionCommandBase txnRequest)
        {
            return new TransactionCreateEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = JToken.FromObject(txnRequest),
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CreateTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CreateTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };
        }

        public static CloudEvent MapVredCreateTxnReqToCloudEvent(VredCreateTransactionEventCommand vredTxnRequest, string eventId)
        {
            return new CloudEvent
            {
                Id = eventId,
                Data = vredTxnRequest,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredCreateTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.VredServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredCreateTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };
        }

        public static TransactionCancelEvent MapTxnCancelRequestToCloudEvent(TransactionCommandBase txnCancelRequest)
        {
            return new TransactionCancelEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Data = JToken.FromObject(txnCancelRequest),
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CancelTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CancelTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            }
            ;
        }

        public static CloudEvent MapVredCancelTxnReqToCloudEvent(VredCancelTransactionRequest vredCancelTxnRequest, string eventId)
        {
            return new CloudEvent
            {
                Id = eventId,
                Data = vredCancelTxnRequest,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredCancelTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.VredServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredCancelTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };
        }

        public static TransactionUpdateEvent MapTxnUpdateRequestToCloudEvent(TransactionCommandBase txnRequest)
        {
            return new TransactionUpdateEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = JToken.FromObject(txnRequest),
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateTransaction.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateTransaction.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };
        }

        public static CloudEvent MapVredUpdateTxnPnrReqToCloudEvent(VredUpdateTransactionPnrRequest vredUpdateTxnPnrRequest, string eventId)
        {
            return new CloudEvent
            {
                Id = eventId,
                Data = vredUpdateTxnPnrRequest,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredUpdateTransactionPnr.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.VredServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.VredUpdateTransactionPnr.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.TransactionDocSchemaVersion
            };
        }
    }
}
