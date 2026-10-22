using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class PatchRegisteredUserEventMapper
    {
        public static CloudEvent EventDataMapper(string eventId, PatchRegisteredUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new CloudEvent
            {
                Id = eventId,
                Type = Constants.Application.PatchRegisteredUserEventType,
                Data = request,
                Subject = Constants.Application.PatchRegisteredUserEventSubject,
                Source = Constants.Application.IdentityEventHandlerTopicName,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecVersion
            };
        }
    }
}
