using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateB2CRepoCommandHandler : IRequestHandler<UpdateB2CRepoCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateB2CRepoCommandHandler> _logger;

        public UpdateB2CRepoCommandHandler(IMediator mediator, ILogger<UpdateB2CRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateB2CRepoCommand req, CancellationToken cancellationToken)
        {
            B2CGraphResponse customerResponse = await _mediator.Send(new UnVerifiedMemberValidationCommand(Constants.Application.Users, GetB2CDetailsQuery(req.MemberUpdateDocument)), cancellationToken);

            if (customerResponse.Value != null && customerResponse.Value.Any() && customerResponse.Value.First().AccountEnabled)
            {
                var customer = customerResponse.Value.First();

                var identity = customer.Identities?
                    .FirstOrDefault(i => i?.SignInType == Constants.Application.B2CSignInType);

                if (identity != null && !string.IsNullOrWhiteSpace(req?.MemberUpdateDocument?.NewEmailId))
                    identity.IssuerAssignedId = req.MemberUpdateDocument.NewEmailId;

                customer.ProxyAddresses = Array.Empty<string>();

                return await _mediator.Send(new UpdateB2CServiceCommand(customerResponse), cancellationToken);
            }
            else
            {
                _logger.LogWarning("Invalid Member or Member not present in Azure B2C");
                return false;
            }
        }

        private static string GetB2CDetailsQuery(MemberUpdateRequest memberUpdateRequest)
        {
            string queryParam = !string.IsNullOrEmpty(memberUpdateRequest.MemberNumber) ? memberUpdateRequest.MemberNumber : memberUpdateRequest.CurrentEmailId;

            return $"{"$select = Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled&$filter=Identities/any(x:x/issuer eq '"}{ConfigReader.Get(Environment.B2CDomain)}{"' and issuerAssignedId eq '"}{ HttpUtility.UrlEncode(queryParam)}{"')"}";

        }
    }
}
