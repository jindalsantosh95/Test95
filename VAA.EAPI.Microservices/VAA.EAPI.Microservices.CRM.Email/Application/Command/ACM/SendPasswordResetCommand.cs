using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class SendPasswordResetCommand : IRequest<Response<bool>>
    {
        public string To { get; set; }
        public string Template { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
    }
}
