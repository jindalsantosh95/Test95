using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription.Enums;

namespace VAA.EAPI.Microservices.Security.Mapper.APIMSubscription
{

    public class APIMSubscriptionCommandToEventMapper<T> where T : class
    {
        public static CloudEvent CommandToEventMapper(T apimSubscriptionCommand, APIMSubscriptionEventType subscriptionEventType, string originator, APIMSubscriptionCommandType subscriptionCommandType)
        {
            return new CloudEvent()
            {
                Data = apimSubscriptionCommand,
                Type = EnumOperations.GetEnumDescription((APIMSubscriptionEventType)Enum.Parse(typeof(APIMSubscriptionEventType), subscriptionEventType.ToString())),
                Source = Constants.Application.APIMSubscriptionEventTopicName,
                Originator = originator,
                Subject = EnumOperations.GetEnumDescription((APIMSubscriptionCommandType)Enum.Parse(typeof(APIMSubscriptionCommandType), subscriptionCommandType.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
