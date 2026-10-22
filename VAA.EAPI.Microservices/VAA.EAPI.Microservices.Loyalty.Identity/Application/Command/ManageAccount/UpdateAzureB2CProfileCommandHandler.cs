using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount
{
    public class UpdateAzureB2CProfileCommandHandler : IUpdateAzureB2CProfileCommandHandler
    {
        private readonly ILogger<UpdateAzureB2CProfileCommandHandler> _logger;
        private readonly IUpdateAzureB2CProfileRepoCommandHandler _updateAzureB2CProfileRepoCommandHandler;

        public UpdateAzureB2CProfileCommandHandler(ILogger<UpdateAzureB2CProfileCommandHandler> logger, IUpdateAzureB2CProfileRepoCommandHandler updateAzureB2CProfileRepoCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _updateAzureB2CProfileRepoCommandHandler = updateAzureB2CProfileRepoCommandHandler ?? throw new ArgumentNullException(nameof(updateAzureB2CProfileRepoCommandHandler));
        }

        public async Task<Result<bool>> Handle(UpdateAzureB2CProfileCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var updateData = Json.DeserializeObject<UpdateAzureB2CProfileRequest>((JToken)request.Data);

                if (string.IsNullOrWhiteSpace(updateData.MemberNumber) || string.IsNullOrWhiteSpace(updateData.FirstName) || string.IsNullOrWhiteSpace(updateData.LastName)) 
                    return false;

                if (updateData.AccountStatus != null && string.IsNullOrWhiteSpace(updateData.AccountStatus))
                    return false;

                if (!string.IsNullOrWhiteSpace(updateData.NewEmailId))
                {
                    bool IsValidEmailId = ValidateEmail.Validate(updateData.NewEmailId);
                    if (!IsValidEmailId)
                        return false;
                }

                return await _updateAzureB2CProfileRepoCommandHandler.Handle(new UpdateAzureB2CProfileRepoCommand(updateData), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VAC Azure B2C profile updation failed.");
                return false;
            }
        }
    }
}
