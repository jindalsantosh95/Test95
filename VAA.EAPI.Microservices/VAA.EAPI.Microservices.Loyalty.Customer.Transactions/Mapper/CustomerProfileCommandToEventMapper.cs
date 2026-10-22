using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class CustomerProfileCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(CustomerProfileAuditEvent command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return new CloudEvent()
            {
                Data = command,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CustomerProfileAudit.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CustomerProfileAudit.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.CustomerSearchSchemaVersion
            };
        }
    }
}
