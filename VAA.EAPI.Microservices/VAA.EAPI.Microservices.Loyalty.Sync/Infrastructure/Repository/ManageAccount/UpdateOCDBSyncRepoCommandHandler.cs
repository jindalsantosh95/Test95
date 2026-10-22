using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.ManageAccount
{
    public class UpdateOCDBSyncRepoCommandHandler : IRequestHandler<UpdateOCDBSyncRepoCommand, Result<bool>>
    {
        private readonly ILogger<UpdateOCDBSyncRepoCommandHandler> _logger;
        private readonly IMediator _mediator;

        public UpdateOCDBSyncRepoCommandHandler(IMediator mediator, ILogger<UpdateOCDBSyncRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(UpdateOCDBSyncRepoCommand request, CancellationToken cancellationToken)
        {
            Result<UpdateResponseType> updateLoyaltySyncResponse = await _mediator.Send(new UpdateOCDBSyncServiceCommand(request.UpdateOCDBSyncQuery), cancellationToken);

            if (updateLoyaltySyncResponse.Value == null || updateLoyaltySyncResponse.Value.Error != null)
            {
                _logger.LogError("Update OCDB failure", updateLoyaltySyncResponse.Value);
                return false;
            }

            return true;
        }
    }
}
