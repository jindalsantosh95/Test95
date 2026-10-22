using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc
{
    public static class YoungFlyerParentEmailEventMapper
    {
        public static CloudEvent CreateEvent(YoungFlyerParentEmailCommand request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Constants.Application.YoungFlyerParentEmailEvent,
                Data = request,
                Subject = Constants.Application.YoungFlyerParentEmailEvent,
                Source = Constants.Application.YoungFlyerParentEmailEventTopicName,
                Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
