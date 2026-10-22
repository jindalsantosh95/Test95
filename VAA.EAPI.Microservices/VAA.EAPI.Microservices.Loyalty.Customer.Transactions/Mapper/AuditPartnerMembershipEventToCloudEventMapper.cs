using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class AuditPartnerMembershipEventToCloudEventMapper
    {
        public static CloudEvent CommandToEventMapper(PartnerMembershipAuditEvent eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CardLinkingAudit.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.AuditServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.CardLinkingAudit.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.PartnerMembershipDocSchemaVersion
            };
        }
    }
}
