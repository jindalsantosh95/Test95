using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class FlyingClubMemberPatchCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(FlyingClubMemberCommandBase command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return new CloudEvent()
            {
                Data = command,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateFlyingClubMemberEvent.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateFlyingClubMemberEvent.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.FlyingClubMemberDocSchemaVersion
            };
        }
    }
}
