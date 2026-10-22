using Azure;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;
using VAA.EAPI.Microservices.Inflight.Connectivity.Helper;
using VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces;
using Environment = VAA.EAPI.Microservices.Inflight.Connectivity.Constants.Environment;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Services
{
    public sealed class RegisterDeviceService : IRegisterDeviceService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<RegisterDeviceService> _logger;

        public RegisterDeviceService(IHttpConnector httpConnector, ILogger<RegisterDeviceService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StarlinkClientResponse> RegisterClientAsync(StarlinkClientRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await TokenHelper.GetStarlinkToken(_httpConnector);
                var clientsEndpoint = $"{ConfigReader.Get(Environment.StarlinkSandboxUrl)}{Constants.Application.ClientsEndpoint}";

                var payload = JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        clientId = request.DeviceId,
                        sandboxId = request.SandboxId,
                        expiry = request.Expiry
                    }
                });

                var response = await _httpConnector.PostAsync(
                    clientsEndpoint,
                    new StringContent(payload, Encoding.UTF8, "application/json"),
                    null,
                    token);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                var result = JsonSerializer.Deserialize<StarlinkClientResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new JsonException("Failed to deserialize Starlink client response.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Starlink device registration failed in {Operation}.", nameof(RegisterClientAsync));
                return new StarlinkClientResponse
                {
                    Errors = new List<string>()
                    {
                        ex.Message
                    },
                    IsValid = false
                };
            }
        }
    }
}
