using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using Environment = VAA.EAPI.Microservices.Retail.Lopa.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Lopa.Application.Query
{
    public class AircraftLopaQueryHandler : IRequestHandler<AircraftLopaQuery, string>
    {
        private readonly IAzureBlobService _azureBlobService;

        public AircraftLopaQueryHandler(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<string> Handle(AircraftLopaQuery request, CancellationToken cancellationToken)
        {
            var blobName = $"{request.FleetTypeCode}-{request.EquipmentCode}.Json";

            await _azureBlobService.Download(ConfigReader.Get(Environment.BlobStorageUri), ConfigReader.Get(Environment.ContainerName), blobName, ConfigReader.Get(Environment.SasKey), cancellationToken);

            return await _azureBlobService.BlobStringAsync(blobName, cancellationToken);
        }
    }
}
