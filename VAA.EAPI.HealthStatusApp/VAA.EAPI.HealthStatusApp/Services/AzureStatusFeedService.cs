using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class AzureStatusFeedService : IAzureStatusFeedService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureStatusFeedService> _logger;
        private readonly string rssFeedUrl = "https://azure.status.microsoft/en-gb/status/feed/";

        public AzureStatusFeedService(HttpClient httpClient, ILogger<AzureStatusFeedService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<SyndicationItem>> GetRssFeedAsync()
        {
            const int maxRetries = 3;
            const int retryDelayMs = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                    using var response = await _httpClient.GetAsync(rssFeedUrl, cts.Token);

                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = XmlReader.Create(stream);
                    var feed = SyndicationFeed.Load(reader);

                    return feed?.Items ?? Enumerable.Empty<SyndicationItem>();
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    _logger.LogWarning(ex, $"Attempt {i + 1} failed: {ex.Message}. Retrying in {retryDelayMs}ms.");
                    await Task.Delay(retryDelayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking Azure status feed after {maxRetries} attempts: {ex.Message}");
                    return Enumerable.Empty<SyndicationItem>();
                }
            }

            return Enumerable.Empty<SyndicationItem>();
        }

        public async Task<int> CalculateAzureScore()
        {
            var response = await GetRssFeedAsync();

            if (response == null || !response.Any())
            {
                return 50;
            }
            else if (!response.All(x => x.Title.Text.Contains("Advisory")))
            {
                return -50;
            }
            else
            {
                return 0;
            }
        }

    }

}
