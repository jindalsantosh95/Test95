using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateAzureB2CProfileRepoCommandHandler : IUpdateAzureB2CProfileRepoCommandHandler
    {

        private readonly UnVerifiedMemberValidationServiceHandler _unVerifiedMemberValidationServiceHandler;
        private readonly UpdateB2CServiceCommandHandler _updateB2CServiceCommandHandler;

        public UpdateAzureB2CProfileRepoCommandHandler(UnVerifiedMemberValidationServiceHandler unVerifiedMemberValidationServiceHandler, UpdateB2CServiceCommandHandler updateB2CServiceCommandHandler)
        {
            _unVerifiedMemberValidationServiceHandler = unVerifiedMemberValidationServiceHandler ?? throw new ArgumentNullException(nameof(unVerifiedMemberValidationServiceHandler));
            _updateB2CServiceCommandHandler = updateB2CServiceCommandHandler ?? throw new ArgumentNullException(nameof(updateB2CServiceCommandHandler));
        }

        public  async Task<bool> Handle(UpdateAzureB2CProfileRepoCommand req, CancellationToken cancellationToken)
        {

            B2CGraphResponse customerResponse = await _unVerifiedMemberValidationServiceHandler.Handle(new UnVerifiedMemberValidationCommand(Constants.Application.Users, GetB2CDetailsQuery(req.updateAzureB2CProfileRequest)), cancellationToken);

            if (customerResponse.Value != null && customerResponse.Value.Any() && customerResponse.Value.First().AccountEnabled)
            {
                customerResponse.Value.First().Identities.FirstOrDefault(identity => identity?.SignInType == Constants.Application.B2CSignInType).IssuerAssignedId = req?.updateAzureB2CProfileRequest?.NewEmailId;

                customerResponse.Value.First().Surname = req?.updateAzureB2CProfileRequest?.LastName;

                customerResponse.Value.First().GivenName = req?.updateAzureB2CProfileRequest?.FirstName;

                B2CExtensionHelper.SetExtensionValue(customerResponse.Value.First(), "memberStatus", req?.updateAzureB2CProfileRequest?.AccountStatus);

                return await _updateB2CServiceCommandHandler.Handle(new UpdateB2CServiceCommand(customerResponse), cancellationToken);

            }

            return false;

        }

        private static string GetB2CDetailsQuery(UpdateAzureB2CProfileRequest updateAzureB2CProfileRequest)
        {
            string queryParam = !string.IsNullOrEmpty(updateAzureB2CProfileRequest.MemberNumber) ? updateAzureB2CProfileRequest.MemberNumber : updateAzureB2CProfileRequest.CurrentEmailId;

            return $"$select=Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled,{ConfigReader.Get(Constants.Environment.ExtensionPrefix)}memberStatus&$filter=Identities/any(x:x/issuer eq '{ConfigReader.Get(Constants.Environment.B2CDomain)}' and issuerAssignedId eq '{queryParam}')";

        }
    }
}
