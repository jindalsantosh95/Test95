using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class IdentitySubToCommandMapper
    {
        public static IdentityEventProcessCommand IdentityEventRequestMapper(CloudEvent identityRequestEventMessage)
        {
            return new IdentityEventProcessCommand
            {
                Id = identityRequestEventMessage.Id,
                Data = identityRequestEventMessage.Data,
                Type = identityRequestEventMessage.Type,
                Subject = identityRequestEventMessage.Subject,
                Specversion = identityRequestEventMessage.Specversion
            };
        }
        
    }
}
