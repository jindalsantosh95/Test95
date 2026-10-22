using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Sockets;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Domain;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Helper;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces;
using Environment = VAA.EAPI.Microservices.Inflight.ConnPulse.Constants.Environment;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Services
{
    public sealed class HeartbeatService : IHeartbeatService
    {
        private const int MaxStarlinkAttempts = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(250);

        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HeartbeatService> _logger;

        public HeartbeatService(IHttpConnector httpConnector, ILogger<HeartbeatService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendHeartbeatAsync(StarlinkHeartbeatRequest request, CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= MaxStarlinkAttempts; attempt++)
            {
                try
                {
                    var token = await TokenHelper.GetStarlinkToken(_httpConnector);
                    var heartbeatEndpoint = $"{ConfigReader.Get(Environment.StarlinkSandboxUrl)}{Constants.Application.HeartbeatEndpoint}";

                    if (!request.Health)
                    {
                        _logger.LogError("Sending unhealthy heartbeat to Starlink");
                    }

                    var payload = JsonSerializer.Serialize(new
                    {
                        healthy = request.Health
                    });

                    var response = await _httpConnector.PutAsync<HttpResponseMessage>(
                        heartbeatEndpoint,
                        new StringContent(payload, Encoding.UTF8, "application/json"),
                        null,
                        token);

                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex) when (IsTransient(ex) && attempt < MaxStarlinkAttempts)
                {
                    _logger.LogWarning(
                        ex,
                        "Transient error polling Starlink heartbeat (attempt {Attempt} of {MaxAttempts}). Retrying in {RetryDelayMs}ms.",
                        attempt,
                        MaxStarlinkAttempts,
                        RetryDelay.TotalMilliseconds);

                    await Task.Delay(RetryDelay, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Polling the Starlink health check failed.");
                }
            }
            return;
        }

        public static bool IsTransient(Exception exception)
        {
            if (exception is HttpRequestException)
            {
                return true;
            }

            if (exception is SocketException socketException &&
                socketException.SocketErrorCode is SocketError.ConnectionReset or SocketError.TimedOut)
            {
                return true;
            }

            return exception.InnerException is not null && IsTransient(exception.InnerException);
        }
    }
}
