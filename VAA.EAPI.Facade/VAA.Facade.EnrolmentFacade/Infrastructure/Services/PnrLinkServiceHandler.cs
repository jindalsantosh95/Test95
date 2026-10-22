using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Helper;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Services
{
    public class PnrLinkServiceHandler : IPnrLinkServiceHandler
    {
        private readonly ILogger<PnrLinkServiceHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PnrLinkServiceHandler(ILogger<PnrLinkServiceHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            
        }

        public async Task<string> Handle(PnrLinkService request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(ConfigReader.GetConfigurationSettings(Constants.Constants.PnrLinkMutationEndpoint));

            var graphQLPayload = new
            {
                query = Constants.Constants.PnrLinkMutationQuery,
                variables = new
                {
                    request
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonContent = JsonSerializer.Serialize(graphQLPayload, options);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");           

            try
            {
                var response = await client.PostAsync("", httpContent, cancellationToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending GraphQL request for Pnr Stamping: {ex.Message}");
                throw;
            }
        }
    }
}
