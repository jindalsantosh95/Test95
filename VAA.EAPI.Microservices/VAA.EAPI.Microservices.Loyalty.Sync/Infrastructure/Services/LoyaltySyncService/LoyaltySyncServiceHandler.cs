using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService
{
    public class LoyaltySyncServiceHandler : IRequestHandler<LoyaltySyncServiceCommand, Result<AddResponseType>>
    {
        private readonly LoyaltyProfileSyncClient _loyaltyProfileSync;
        private readonly ILogger<LoyaltySyncServiceHandler> _logger;

        public LoyaltySyncServiceHandler(LoyaltyProfileSyncClient loyaltyProfileSync, ILogger<LoyaltySyncServiceHandler> logger)
        {
            _loyaltyProfileSync = loyaltyProfileSync ?? throw new ArgumentNullException(nameof(loyaltyProfileSync));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<AddResponseType>> Handle(LoyaltySyncServiceCommand syncRequest, CancellationToken cancellationToken)
        {
            AddRequestType memberSyncInput = LoyaltySyncRequestMapper.ToOcdbSyncRequest(syncRequest);
            string json = Json.Serialize(memberSyncInput);

            if (_loyaltyProfileSync.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _loyaltyProfileSync.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            AddResponse responseWcf = await _loyaltyProfileSync.AddAsync(memberSyncInput);
            _logger.LogInformation($"DeltaOCDB.LoyaltyProfileSync Response : {Json.Serialize(responseWcf?.AddResponse1)}");
            return responseWcf?.AddResponse1;
        }
    }
}
