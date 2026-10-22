using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper
{
    public class AddressLookupCommandRequestMapper
    {
        public static AddressLookupEventProcessCommand EventRequestMapper(CloudEvent addressLookupRequestEventMessage)
        {
            return new AddressLookupEventProcessCommand
            {
                Id = addressLookupRequestEventMessage.Id,
                Data = addressLookupRequestEventMessage.Data,
                Type = addressLookupRequestEventMessage.Type,
                Subject = addressLookupRequestEventMessage.Subject,
                Specversion = addressLookupRequestEventMessage.Specversion
            };
        }
    }
}
