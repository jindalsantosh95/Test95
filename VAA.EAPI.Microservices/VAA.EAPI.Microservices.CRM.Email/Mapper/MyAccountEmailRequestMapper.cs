using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class MyAccountEmailRequestMapper
    {
        public static AdobeEmailCommand EventRequestMapper(AdobeEmailCommand emailRequestEventMessage)
        {
            return new AdobeEmailCommand
            {
                To = emailRequestEventMessage.To,
                Template = emailRequestEventMessage.Template
            };
        }
    }
}
