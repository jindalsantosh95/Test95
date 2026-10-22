using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class OTPEmailCommandRequestMapper
    {
        public static OTPEmailCommand EventRequestMapper(CloudEvent emailRequestEventMessage)
        {
            return new OTPEmailCommand
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
