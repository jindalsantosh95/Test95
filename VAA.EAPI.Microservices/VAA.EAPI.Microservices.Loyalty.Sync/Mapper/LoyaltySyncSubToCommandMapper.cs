using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Mapper
{
    public class LoyaltySyncSubToCommandMapper
    {
        public static LoyaltySyncRequest LoyaltySyncEventRequestMapper(CloudEvent LoyaltySyncRequestEventMessage)
        {
            return new LoyaltySyncRequest
            {
                Id = new Guid(LoyaltySyncRequestEventMessage.Id),
                Data = (LoyaltySyncRequestData)LoyaltySyncRequestEventMessage.Data,
                Type = LoyaltySyncRequestEventMessage.Type,
                Subject = LoyaltySyncRequestEventMessage.Subject,
                Source = LoyaltySyncRequestEventMessage.Source,
                Specversion = LoyaltySyncRequestEventMessage.Specversion
            };
        }
    }
}
