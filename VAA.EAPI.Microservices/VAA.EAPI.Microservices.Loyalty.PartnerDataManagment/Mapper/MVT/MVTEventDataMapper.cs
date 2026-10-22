using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper.MVT
{
    public class MVTEventDataMapper
    {
        public static CloudEvent MVTMessageEventDataMapper(MVTMessageRequest message)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.FlightMessageEventType,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = message,
                Source = Constants.Application.FlightMessageTopicName,
                Type = Constants.Application.FlightMessageEventType,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
