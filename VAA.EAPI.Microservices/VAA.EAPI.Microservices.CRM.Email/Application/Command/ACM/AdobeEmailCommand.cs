using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class AdobeEmailCommand : IRequest<Response<bool>>
    {
        public string To { get; set; }
        public string Template { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }

        public static explicit operator AdobeEmailCommand(SendPasswordResetCommand req)
        {
            return new()
            {
                To = req.To,
                Template = req.Template,
                FirstName = req.FirstName,
                TierCode = req.TierCode
            };
        }
    }
}
