using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.ManageAccount
{
    public class UpdateOCDBSyncServiceCommandHandler : IRequestHandler<UpdateOCDBSyncServiceCommand, Result<UpdateResponseType>>
    {
        private readonly LoyaltyProfileSyncClient _loyaltyProfileSync;
        private readonly ILogger<UpdateOCDBSyncServiceCommandHandler> _logger;

        public UpdateOCDBSyncServiceCommandHandler(LoyaltyProfileSyncClient loyaltyProfileSync, ILogger<UpdateOCDBSyncServiceCommandHandler> logger)
        {
            _loyaltyProfileSync = loyaltyProfileSync ?? throw new ArgumentNullException(nameof(loyaltyProfileSync));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<UpdateResponseType>> Handle(UpdateOCDBSyncServiceCommand syncRequest, CancellationToken cancellationToken)
        {
            try
            {
                UpdateRequestType memberSyncInput = UpdateOCDBRequestMapper.ToOcdbSyncRequest(syncRequest);

                if (_loyaltyProfileSync.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _loyaltyProfileSync.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                return (await _loyaltyProfileSync.UpdateAsync(memberSyncInput)).UpdateResponseType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update OCDB Sync Failure", ex);
                return new Result<UpdateResponseType>();
            }

        }
    }
}
