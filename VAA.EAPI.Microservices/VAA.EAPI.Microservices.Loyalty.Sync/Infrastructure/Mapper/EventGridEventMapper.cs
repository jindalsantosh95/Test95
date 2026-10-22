using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper
{
    public class EventGridEventMapper
    {
        public static EventGridTopicEvent EventDataMapper(TransactionalEvent eventdata)
        {
            return new EventGridTopicEvent
            {
                Id = eventdata.Id,
                Subject = eventdata.Subject,
                Type = eventdata.EventType,
                Data = eventdata.Data,
                Source = ConfigReader.Get(Constants.Environment.EventGridUrl),
                Key = ConfigReader.Get(Constants.Environment.EventGridTopicKey),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
