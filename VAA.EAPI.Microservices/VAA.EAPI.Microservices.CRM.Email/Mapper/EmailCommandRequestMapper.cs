using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class EmailCommandRequestMapper
    {
        public static EmailEventProcessCommand EventRequestMapper(CloudEvent emailRequestEventMessage)
        {
            return new EmailEventProcessCommand
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
