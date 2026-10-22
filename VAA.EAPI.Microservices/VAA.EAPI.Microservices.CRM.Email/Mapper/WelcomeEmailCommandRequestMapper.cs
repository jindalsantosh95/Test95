using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class WelcomeEmailCommandRequestMapper
    {
        public static WelcomeEmailEventProcessCommand EventRequestMapper(CloudEvent emailRequestEventMessage)
        {
            return new WelcomeEmailEventProcessCommand
            {
                Id = emailRequestEventMessage.Id,
                Data = emailRequestEventMessage.Data,
                Type = emailRequestEventMessage.Type,
                Subject = emailRequestEventMessage.Subject,
                Specversion = emailRequestEventMessage.Specversion
            };
        }
    }
}
