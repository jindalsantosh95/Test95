using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class ActionEventMapper
    {
        public static CloudEvent MapActionEvent(RuleActionsResponse eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.RuleEngineActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((RuleEngineEventType)Enum.Parse(typeof(RuleEngineEventType), RuleEngineEventType.RuleEngineActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
