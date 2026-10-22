using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Services
{
    public class TravelDestinations(ILogger<TravelDestinations> logger, IAzureBlobService azureBlobService) : ITravelDestinations
    {
        private readonly ILogger<TravelDestinations> _logger = logger;
        private readonly IAzureBlobService _azureBlobService = azureBlobService;

        public async Task<IEnumerable<string>> RetriveTravelDestinations()
        {
            try
            {
                await _azureBlobService.DownloadBlobAsync(ConfigReader.Get(Constants.Environment.BlobStorageUri), ConfigReader.Get(Constants.Environment.ContainerName), ConfigReader.Get(Constants.Environment.TravelDestinationBlobName), ConfigReader.Get(Constants.Environment.SasKey), CancellationToken.None);

                return await _azureBlobService.BlobJsonAsAsync<IEnumerable<string>>(ConfigReader.Get(Constants.Environment.TravelDestinationBlobName), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve travel destinations from blob storage");
                return [];
            }
        }
    }
}
