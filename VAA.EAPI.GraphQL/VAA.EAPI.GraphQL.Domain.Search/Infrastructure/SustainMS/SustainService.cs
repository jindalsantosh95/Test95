using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Sustain;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.SustainMS
{
    public class SustainService : ISustainService
    {
        protected readonly IHttpConnector _connector;
        private readonly ILogger<SustainService> _logger;
        private static readonly Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.SustainMSOcpKey) } };

        public SustainService(IHttpConnector connector, ILogger<SustainService> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<SustainMSResponse>> RetrieveSustainResponseAsync(SustainMSRequest sustainMSRequest)
        {
            return (await _connector.SendAsync<IEnumerable<SustainMSResponse>>(ConfigReader.Get(Constants.Environment.SustainMsUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(sustainMSRequest))).Response;
        }

        public async Task<IEnumerable<IEnumerable<SustainMSResponse>>> RetrieveSustainResponseCollectionAsync(IEnumerable<SustainMSRequest> sustainMSRequests)
        {
            var sustainMSResponseTasks = sustainMSRequests.Select(async sustainMSRequest =>
            {
                try
                {
                    return await RetrieveSustainResponseAsync(sustainMSRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in Sustain MS service call for the request: {Json.Serialize(sustainMSRequest)}");
                    return null;
                }
            });

            return (await Task.WhenAll(sustainMSResponseTasks)).Where(response => response != null);
        }

    }
}