using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class ValidatePasswordCommand : IRequest<Response<IEnumerable<PasswordPolicy>>>
    {
        public PasswordPolicyRequest Password { get; set; }

    }
}
