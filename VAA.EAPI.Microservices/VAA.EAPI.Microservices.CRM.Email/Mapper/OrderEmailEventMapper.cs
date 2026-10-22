using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class OrderEmailEventMapper
    {
        public static CloudEvent EventMapper(OrderEmailCommand request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = Json.Serialize(request),
                Type = Constants.Application.OrderEmailEventType,
                Subject = Constants.Application.OrderEmailEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat)
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }
    }
}
