using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public sealed class MetaPartnerEventCommand : CloudEvent
    {
        public static explicit operator MetaPartnerEventCommand(MetaPartnerCommand source)
        {
            var cloudEvent = new MetaPartnerEventCommand()
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.Application.MetaPartnerEventType,
                Subject = Constants.Application.MetaPartnerEventSubject,
                Source = Constants.Application.MetaPartnerEventHandlerTopicName,
                Data = Json.Serialize(source).Compress(),
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }
    }
}
