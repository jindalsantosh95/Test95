using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateAzureB2CRepoManageAccountCommandHandler : IRequestHandler<UpdateAzureB2CRepoManageAccountCommand, bool>
    {

        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAzureB2CRepoManageAccountCommand> _logger;

        public UpdateAzureB2CRepoManageAccountCommandHandler(IMediator mediator, ILogger<UpdateAzureB2CRepoManageAccountCommand> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateAzureB2CRepoManageAccountCommand req, CancellationToken cancellationToken)
        {
            B2CGraphResponse customerResponse = await _mediator.Send(new UnVerifiedMemberValidationCommand(Constants.Application.Users, GetB2CDetailsQuery(req.manageAccountAzureB2CUpdateRequest)), cancellationToken);

            if (customerResponse.Value != null && customerResponse.Value.Any() && customerResponse.Value.First().AccountEnabled)
            {
                customerResponse.Value.First().Identities.FirstOrDefault(identity => identity?.SignInType == Constants.Application.B2CSignInType).IssuerAssignedId = req?.manageAccountAzureB2CUpdateRequest?.NewEmailId;

                customerResponse.Value.First().Surname = req?.manageAccountAzureB2CUpdateRequest?.LastName;

                customerResponse.Value.First().GivenName = req?.manageAccountAzureB2CUpdateRequest?.FirstName;

                return await _mediator.Send(new UpdateB2CServiceCommand(customerResponse), cancellationToken);
            }

            return false;
        }

        private static string GetB2CDetailsQuery(ManageAccountAzureB2CUpdateRequest manageAccountAzureB2CUpdateRequest)
        {
            string queryParam = !string.IsNullOrEmpty(manageAccountAzureB2CUpdateRequest.MemberNumber) ? manageAccountAzureB2CUpdateRequest.MemberNumber : manageAccountAzureB2CUpdateRequest.CurrentEmailId;

            return $"{"$select = Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled&$filter=Identities/any(x:x/issuer eq '"}{ConfigReader.Get(Constants.Environment.B2CDomain)}{"' and issuerAssignedId eq '"}{queryParam}{"')"}";

        }
    }
}
