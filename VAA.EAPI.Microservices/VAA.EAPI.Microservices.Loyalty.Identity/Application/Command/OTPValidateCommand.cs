using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class OTPValidateCommand : IRequest<Response<string>>
    {
        public string EmailId { get; set; }
        public string Otp { get; set; }
    }

}
