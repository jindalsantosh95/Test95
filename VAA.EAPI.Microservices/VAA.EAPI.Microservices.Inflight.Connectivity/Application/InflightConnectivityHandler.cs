using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;
using VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Application
{

    public sealed class InflightConnectivityHandler : IInflightConnectivityHandler
    {
        private readonly IRegisterDeviceService _starlinkService;
        private readonly ILogger<InflightConnectivityHandler> _logger;


        public InflightConnectivityHandler(IRegisterDeviceService starlinkService, ILogger<InflightConnectivityHandler> logger)
        {
            _starlinkService = starlinkService ?? throw new ArgumentNullException(nameof(starlinkService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<AccountInflightDeviceStateResponse> RegisterClientAsync(InflightConnectivityRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var clientRequest = new StarlinkClientRequest
            {
                DeviceId = request.DeviceId,
                SandboxId = request.SandboxId,
                Expiry = request.Expiry
            };

            var result = await _starlinkService.RegisterClientAsync(clientRequest, cancellationToken);

            if (result != null && result.IsValid && (result.Errors == null || !result.Errors.Any()))
            {
                return new AccountInflightDeviceStateResponse() { Status = "Registered" };
            }

            var errorText =
                result == null
                    ? "Result was null"
                    : (result.Errors == null || !result.Errors.Any()
                        ? "No error details provided"
                        : string.Join(", ", result.Errors));

            _logger.LogError("Wi-Fi device registration returned an unsuccessful result. Errors: {Errors}", errorText);
            return new AccountInflightDeviceStateResponse() { Status = "Failed" };
        }
    }
}
