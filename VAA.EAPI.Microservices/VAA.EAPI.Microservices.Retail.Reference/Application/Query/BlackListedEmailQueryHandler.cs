using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class BlackListedEmailQueryHandler : IRequestHandler<BlackListedEmailQuery, string>
    {
        private readonly IAzureBlobService _azureBlobService;

        public BlackListedEmailQueryHandler(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<string> Handle(BlackListedEmailQuery request, CancellationToken cancellationToken)
        {
            var blobName = ConfigReader.Get(Environment.BlackListEmailBlobName);

            await _azureBlobService.Download(ConfigReader.Get(Environment.LoyaltyBlobStorageUri), ConfigReader.Get(Environment.BlackListEmailContainerName), blobName, ConfigReader.Get(Environment.LoyaltyBlobStorageSaSKey), cancellationToken);

            return await _azureBlobService.BlobStringAsync(blobName, cancellationToken);
        }
    }
}
