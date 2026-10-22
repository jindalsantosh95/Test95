using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Application.PricingFeed;

public sealed class GeneratePricingFeedService( ILogger<GeneratePricingFeedService> logger, IFederatedDataService federatedDataService, ITravelDestinations travelDestinations, IAzureBlobService azureBlobService) : IGeneratePricingFeedService
{
    private readonly ILogger<GeneratePricingFeedService> _logger = logger;
    private readonly IFederatedDataService _federatedDataService = federatedDataService;
    private readonly ITravelDestinations _travelDestinations = travelDestinations;
    private readonly IAzureBlobService _azureBlobService = azureBlobService;

    public async Task PublishAsync()
    {
        try
        {
            if (ConfigReader.Get<bool>(Constants.Environment.EnablePriceFeed))
            {

                var destinations = await _travelDestinations.RetriveTravelDestinations();

                foreach (var destination in destinations)
                {
                    try
                    {

                        var federatedResponse = await _federatedDataService.GetFederatedDataAsync(destination);
                        var mappedRows = federatedResponse?.Holidays?.Where(h => h?.Hotel?.Content?.Location != null && h.SpecialOffers != null).Select(h => (GenaratePricingFeedDataResponse)h) ?? [];

                        if (!mappedRows.Any())
                        {
                            _logger.LogWarning("No data for destination {Destination}", destination);
                            continue;
                        }

                        var csv = BuildCsv(mappedRows);

                        var fileName = $"VAH_Price_Feed_for_Marketing_{destination}.csv";

                        await _azureBlobService.UploadBlobAsync(ConfigReader.Get<string>(Constants.Environment.VholsBlobStorageUrl), ConfigReader.Get<string>(Constants.Environment.VholsBlobContainer), ConfigReader.Get<string>(Constants.Environment.VholsBlobSasKey), fileName, csv, BlobType.Json, overwrite: true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing destination {Destination}", destination);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
    

    private static string BuildCsv(IEnumerable<Domain.GenaratePricingFeedDataResponse> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(PricingFeedCsvBuilder.Header);

        foreach (var row in rows)
            sb.AppendLine(PricingFeedCsvBuilder.ToCsvLine(row));

        return sb.ToString();
    }
}
