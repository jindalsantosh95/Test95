using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class DuplicateEmailIdCheckCommandHandler : IRequestHandler<DuplicateEmailIdCheckCommand, bool>
    {
        private readonly IMediator _mediator;

        public DuplicateEmailIdCheckCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(DuplicateEmailIdCheckCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new UnVerifiedMemberValidationCommand("/users", GetB2CQuery(request.EmailID)), cancellationToken);
            return response?.Value?.FirstOrDefault()?.Identities?.Any() ?? false;
        }

        private static string GetB2CQuery(string emailID)
        {
            if (!string.IsNullOrEmpty(emailID))
                return $"$select=id,userPrincipalName,accountEnabled,mail,Identities" +
            $"&$filter={ConfigReader.Get(Environment.Identities)}/any(x: x / issuer eq {"'"}{ConfigReader.Get(Environment.B2CDomain)}{"'"} and issuerAssignedId eq {"'"}{emailID}{"'"})";
            return string.Empty;
        }
    }
}
