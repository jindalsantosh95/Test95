using Microsoft.Extensions.Configuration;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google;

public partial class GoogleService : IGoogleService
{
    private const int MaxDestinationsPerRequest = 25;
    private const string DefaultDistanceMatrixBaseUrl = "https://maps.googleapis.com/maps/api/distancematrix/json";
    private readonly IHttpConnector _httpConnector;
    private readonly IConfiguration _configuration;

    public GoogleService(IHttpConnector httpConnector, IConfiguration configuration)
    {
        _httpConnector = httpConnector;
        _configuration = configuration;
    }

    public async Task<IReadOnlyDictionary<string, DistanceMatrixResult>> GetDistanceMetricsAsync(string origin, IReadOnlyCollection<StoreResult> stores)
    {
        if (stores.Count == 0)
        {
            return new Dictionary<string, DistanceMatrixResult>();
        }

        var apiKey = _configuration[Constants.Environment.GoogleDistanceMatrixApiKey] ?? string.Empty;
        var baseUrl = _configuration[Constants.Environment.GoogleDistanceMatrixBaseUrl] ?? DefaultDistanceMatrixBaseUrl;
        var storeArray = stores.ToArray();
        var result = storeArray.ToDictionary(store => store.StoreId, _ => new DistanceMatrixResult
        {
            DistanceInMeters = DistanceMatrixResult.UnknownDistanceInMeters
        }, StringComparer.OrdinalIgnoreCase);

        var batchTasks = storeArray
            .Chunk(MaxDestinationsPerRequest)
            .Select(batch => GetDistanceBatchAsync(origin, batch, baseUrl, apiKey))
            .ToArray();

        var batchResponses = await Task.WhenAll(batchTasks);

        foreach (var batchResponse in batchResponses)
        {
            for (var index = 0; index < batchResponse.Stores.Length; index++)
            {
                var store = batchResponse.Stores[index];
                var element = batchResponse.Row?.Elements.ElementAtOrDefault(index);

                if (element is null || !string.Equals(element.Status, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result[store.StoreId] = new DistanceMatrixResult
                {
                    DistanceInMeters = element.Distance?.Value ?? DistanceMatrixResult.UnknownDistanceInMeters,
                    DistanceText = element.Distance?.Text ?? string.Empty,
                    DurationText = element.Duration?.Text ?? string.Empty
                };
            }
        }

        return result;
    }

    private async Task<DistanceMatrixBatchResponse> GetDistanceBatchAsync(string origin, StoreResult[] stores, string baseUrl, string apiKey)
    {
        var destinations = string.Join('|', stores.Select(BuildDestination));
        var uri = $"{baseUrl}?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(destinations)}&units=imperial&key={Uri.EscapeDataString(apiKey)}";
        var response = await _httpConnector.GetAsync<GoogleDistanceMatrixApiResponse>(uri, new Dictionary<string, string>());

        return new DistanceMatrixBatchResponse(stores, response.Rows.FirstOrDefault());
    }

    private static string BuildDestination(StoreResult store) => $"{store.Latitude:0.######},{store.Longitude:0.######}";
}