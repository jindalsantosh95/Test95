using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository
{
    public class LoyaltySyncRepoHandler : IRequestHandler<LoyaltySyncRepoCommand, Result<bool>>
    {
        private readonly ILogger<LoyaltySyncRepoHandler> _logger;
        private readonly IMediator _mediator;

        public LoyaltySyncRepoHandler(IMediator mediator, ILogger<LoyaltySyncRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(LoyaltySyncRepoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loyalty Sync repo handler");
            Result<AddResponseType> LoyaltySyncResponse = await _mediator.Send(new LoyaltySyncServiceCommand(request), cancellationToken);
            _logger.LogInformation("DeltaOCDB.memberSync response : \n " + LoyaltySyncResponse);
            if (LoyaltySyncResponse.Value != null && LoyaltySyncResponse.Value.AdvisoryMessage != null && LoyaltySyncResponse.Value.Error == null)
            {
                _logger.LogInformation("LoyaltyProfileSyncResponse - Success Response");
                return true;
            }
            else
            {
                _logger.LogInformation("LoyaltyProfileSyncResponse - Error Response");
                return false;
            }
        }
    }
}
