using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Transaction;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class ActionExecutionEventMapper
    {
        public static CloudEvent MapFCMDocEvent(FlyingClubMemberUpdateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.FcmUpdateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerTransactionEventTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.FcmUpdateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
        public static CloudEvent MapMembershipDocUpdateEvent(MembershipChangeDocumentEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.MembershipChangeUpdateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerTransactionEventTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.MembershipChangeUpdateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
        public static CloudEvent MapMembershipDocCreateEvent(MembershipChangeDocumentEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.MembershipChangeCreateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerTransactionEventTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.MembershipChangeCreateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
        public static CloudEvent MapFCMDocEventFlags(FlyingClubMemberUpdateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.FcmUpdateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerTransactionEventTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.FcmUpdateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }

        public static CloudEvent MapTxnDocCreateEvent(TransactionCreateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.TransactionCreateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.CustomerTransactionEventTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.TransactionCreateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
