using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{

    public class ValidatePasswordCommandHandler : IRequestHandler<ValidatePasswordCommand, Response<IEnumerable<PasswordPolicy>>>
    {
        public async Task<Response<IEnumerable<PasswordPolicy>>> Handle(ValidatePasswordCommand request, CancellationToken cancellationToken)
        {
            return new Response<IEnumerable<PasswordPolicy>>()
            {
                Status = Status.OK,
                Value = await PasswordPolicyValidator.Verify(request.Password.Password),
                Errors = null
            };
        }
    }
}
