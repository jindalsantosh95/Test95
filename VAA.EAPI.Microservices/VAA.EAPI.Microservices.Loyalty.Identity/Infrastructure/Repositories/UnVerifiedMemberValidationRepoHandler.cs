using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UnVerifiedMemberValidationRepoHandler : IRequestHandler<UnVerifiedMemberRepoCommand, string>
    {
        private readonly IMediator _mediator;

        public UnVerifiedMemberValidationRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<string> Handle(UnVerifiedMemberRepoCommand request, CancellationToken cancellationToken)
        {
            B2CGraphResponse response = await _mediator.Send(new UnVerifiedMemberValidationCommand("/users", GetB2CQuery(request.MemberLoginRequest)), cancellationToken);

            //Validating an account is Enabled while Login 
            if (response == null || (response.Value.Count > 0 && (!response.Value.FirstOrDefault().AccountEnabled)))
            {
                return Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.UnverifiedAccount, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
            }

            return string.Empty;
        }

        private static string GetB2CQuery(MemberLoginRequest memberLoginRequest)
        {
            string queryParam = !string.IsNullOrEmpty(memberLoginRequest.MemberNumber) ? memberLoginRequest.MemberNumber : memberLoginRequest.EmailAddress;
            return $"$select=Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled" +
                $"&$filter=Identities/any(x:x/issuer eq '" + ConfigReader.Get(Environment.B2CDomain) + "' and issuerAssignedId eq '" + HttpUtility.UrlEncode(queryParam) + "')";

        }
    }
}
