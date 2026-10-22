using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount
{
    public class UpdateAzureB2CManageAccountCommandHandler: IRequestHandler<UpdateAzureB2CManageAccountCommand, Result<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAzureB2CManageAccountCommandHandler> _logger;

        public UpdateAzureB2CManageAccountCommandHandler(IMediator mediator, ILogger<UpdateAzureB2CManageAccountCommandHandler> logger)
        {
            _mediator = mediator?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(UpdateAzureB2CManageAccountCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            try
            {
                var updateData = Json.DeserializeObject<ManageAccountAzureB2CUpdateRequest>((JToken)request.Data);

                bool IsValidEmailId = ValidateEmail.Validate(updateData.NewEmailId);
                if (!IsValidEmailId)
                    return false;

                return await _mediator.Send(new UpdateAzureB2CRepoManageAccountCommand(updateData), cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VAC Manage Account Azure B2C updation failed.");
                return false;
            }
        }
    }
}
