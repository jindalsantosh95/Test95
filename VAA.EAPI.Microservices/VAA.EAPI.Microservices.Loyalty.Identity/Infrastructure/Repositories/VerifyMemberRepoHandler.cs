using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class VerifyMemberRepoHandler : IRequestHandler<VerifyMemberRepoCommand, bool>
    {
        private readonly IMediator _mediator;

        public VerifyMemberRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(VerifyMemberRepoCommand request, CancellationToken cancellationToken)
        {

            B2CGraphResponse customerResponse = await _mediator.Send(new UnVerifiedMemberValidationCommand(Constants.Application.Users, GetB2CDetailsQuery(request.MemberLoginRequest.EmailAddress)), cancellationToken);

            if (customerResponse.Value != null && customerResponse.Value.Any())
                return true;

            return false;
        }

        private static string GetB2CDetailsQuery(string newEmailAddress)
        {
            return $"{"$select = Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled&$filter=Identities/any(x:x/issuer eq '"}{ConfigReader.Get(Environment.B2CDomain)}{"' and issuerAssignedId eq '"}{newEmailAddress}{"')"}";
        }
    }
}
