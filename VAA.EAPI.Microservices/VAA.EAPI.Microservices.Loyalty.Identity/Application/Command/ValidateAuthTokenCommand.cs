using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class ValidateAuthTokenCommand : IRequest<Response<string>>
    {
        public string Token { get; set; }

        public ValidateAuthTokenCommand(string token)
        {
            Token = token;
        }
    }
}
