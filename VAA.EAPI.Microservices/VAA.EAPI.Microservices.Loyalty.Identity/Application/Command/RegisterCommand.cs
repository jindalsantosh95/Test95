using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class RegisterCommand : IRequest<Response<B2CUserResponse>>
    {
        public CustomerAccountDetails CustomerDetails { get; set; }
    }
}
