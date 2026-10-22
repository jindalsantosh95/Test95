#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using OperationsApplication = VAA.EAPI.GraphQL.Domain.Operations.Constants.Application;

namespace VAA.EAPI.GraphQL.Domain.Operations.Services
{
    public sealed class FlightStatusProvider(
        IHttpConnector httpConnector,
        ILogger<FlightStatusProvider> logger,
        TimeProvider? timeProvider = null) : IFlightStatusProvider
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<FlightStatusProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

        public Task<FlightStatusRoute?> SearchByRouteAsync(
            FlightStatusRouteRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            FlightStatusInputValidator.Validate(request, _timeProvider);

            return SendAsync<FlightStatusRoute, FlightStatusRoute>(
                () => BuildRouteUri(request),
                route => route is null || route.Routes is null || !route.Routes.Any(),
                route => route,
                cancellationToken);
        }

        public Task<FlightStatusFlight?> SearchByNumberAsync(
            FlightStatusNumberRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            FlightStatusInputValidator.Validate(request, _timeProvider);

            return SendAsync<FlightStatusInternalNumberResponse, FlightStatusFlight>(
                () => BuildNumberUri(request),
                flight => flight is null ||
                          flight.ActiveLeg is null &&
                          (flight.Legs is null || !flight.Legs.Any(leg => leg is not null)),
                flight => flight is null ? null : FlightStatusFlight.FromInternal(flight, request.DepartureDate),
                cancellationToken);
        }

        private async Task<TPublicResponse?> SendAsync<TWireResponse, TPublicResponse>(
            Func<Uri> buildUri,
            Func<TWireResponse?, bool> isNotFound,
            Func<TWireResponse?, TPublicResponse?> mapResponse,
            CancellationToken cancellationToken)
        {
            try
            {
                using var requestDeadline = CreateRequestDeadline(cancellationToken);
                var response = await SendStatusRequestAsync<TWireResponse>(
                    buildUri(),
                    requestDeadline.Token);
                var statusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode && statusCode != HttpStatusCode.RedirectMethod)
                {
                    var failure = MapFailure(statusCode);
                    var exception = new FlightStatusProviderException(failure);
                    if (failure is FlightStatusProviderFailure.Timeout or FlightStatusProviderFailure.Unavailable)
                        _logger.LogError(exception, "Flight-status dependency returned status {StatusCode}.", (int)statusCode);
                    throw exception;
                }

                if (isNotFound(response.Response))
                {
                    throw new FlightStatusProviderException(FlightStatusProviderFailure.NotFound);
                }

                return mapResponse(response.Response);
            }
            catch (FlightStatusProviderException)
            {
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogError(exception, "Flight-status dependency request timed out.");
                throw new FlightStatusProviderException(FlightStatusProviderFailure.Timeout, exception);
            }
            catch (HttpRequestException exception)
            {
                var statusCode = exception.StatusCode;
                var failure = statusCode is { } value
                    ? MapFailure(value)
                    : FlightStatusProviderFailure.Unavailable;
                _logger.LogError(exception, "Flight-status dependency HTTP request failed with status {StatusCode}.", (int?)statusCode);
                throw new FlightStatusProviderException(failure, exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Flight-status dependency request failed.");
                throw new FlightStatusProviderException(FlightStatusProviderFailure.Unavailable, exception);
            }
        }

        private async Task<(HttpStatusCode StatusCode, bool IsSuccessStatusCode, TResponse? Response)> SendStatusRequestAsync<TResponse>(
            Uri uri,
            CancellationToken cancellationToken)
        {
            var attemptTimeoutSeconds = Math.Clamp(
                ConfigReader.Try("FlightStatus:GraphQLAttemptTimeoutSeconds", 25),
                1,
                120);
            var maxResponseBytes = Math.Clamp(
                ConfigReader.Try("FlightStatus:MaxResponseBytes", 4 * 1024 * 1024),
                1024,
                16 * 1024 * 1024);
            using var attemptDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attemptDeadline.CancelAfter(TimeSpan.FromSeconds(attemptTimeoutSeconds));
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Accept"] = "application/json",
                ["Cache-Control"] = "no-cache",
                ["x-nowait"] = "true",
                [VAA.EAPI.GraphQL.Common.Constants.Application.OcpKeyHeader] = ConfigReader.Get(OperationsApplication.FlightStatusSubscriptionKey)
            };
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            using var response = await _httpConnector.SendAsync(request, attemptDeadline.Token);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.RedirectMethod ||
                response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.RedirectMethod)
            {
                return (response.StatusCode, response.IsSuccessStatusCode, default);
            }

            var content = await ReadContentAsync(response, maxResponseBytes, attemptDeadline.Token);
            var deserialized = content.Length == 0
                ? default
                : Json.Deserialize<TResponse>(Encoding.UTF8.GetString(content));

            return (response.StatusCode, response.IsSuccessStatusCode, deserialized);
        }

        private static async Task<byte[]> ReadContentAsync(
            HttpResponseMessage response,
            int maxResponseBytes,
            CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength is long contentLength && contentLength > maxResponseBytes)
                throw new InvalidDataException("Flight-status service response exceeded the configured size limit.");

            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var output = new MemoryStream(Math.Min(maxResponseBytes, 8192));
            var buffer = new byte[8192];

            while (true)
            {
                var remaining = maxResponseBytes - (int)output.Length;
                var bytesRead = await input.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, remaining + 1)), cancellationToken);
                if (bytesRead == 0)
                    break;

                if (bytesRead > remaining)
                    throw new InvalidDataException("Flight-status service response exceeded the configured size limit.");

                await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            return output.ToArray();
        }

        private static CancellationTokenSource CreateRequestDeadline(CancellationToken cancellationToken)
        {
            var timeoutSeconds = Math.Clamp(ConfigReader.Try("FlightStatus:GraphQLTimeoutSeconds", 75), 1, 120);
            var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return deadline;
        }

        private static FlightStatusProviderFailure MapFailure(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.NotFound => FlightStatusProviderFailure.NotFound,
            HttpStatusCode.BadRequest => FlightStatusProviderFailure.InvalidInput,
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => FlightStatusProviderFailure.Timeout,
            _ => FlightStatusProviderFailure.Unavailable
        };

        private static Uri BuildRouteUri(FlightStatusRouteRequest request)
        {
            var path = $"flightstatus/routes/{Uri.EscapeDataString(request.DepartureAirportCode.Trim().ToUpperInvariant())}/{Uri.EscapeDataString(request.ArrivalAirportCode.Trim().ToUpperInvariant())}/{request.ScheduledDepartureDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
            var query = new List<string>();
            var timeRange = request.TimeRange?.ToArray();
            var stops = request.Stops?.ToArray();

            if (timeRange is { Length: > 0 })
                query.Add($"timeRange={Uri.EscapeDataString(string.Join(",", timeRange))}");

            if (stops is { Length: > 0 })
                query.Add($"stops={Uri.EscapeDataString(string.Join(",", stops))}");

            if (request.First.HasValue)
                query.Add($"first={(request.First.Value ? "true" : "false")}");

            return BuildInternalApiUri(path, query);
        }

        private static Uri BuildNumberUri(FlightStatusNumberRequest request)
        {
            var path = $"flightstatus/flights/{Uri.EscapeDataString(request.FlightNumber.Trim())}/{request.DepartureDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
            var query = new List<string>();

            query.Add($"departureAirportCode={Uri.EscapeDataString(request.DepartureAirportCode.Trim().ToUpperInvariant())}");
            AddOptionalQueryParameter(query, "selectedLegId", NormalizeOptional(request.SelectedLegId));

            return BuildInternalApiUri(path, query);
        }

        private static Uri BuildInternalApiUri(string relativePath, IReadOnlyCollection<string> query)
        {
            var configuredBaseUrl = ConfigReader.Get(OperationsApplication.FlightStatusInternalApiUrl);
            if (!Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var baseUri) ||
                !baseUri.Scheme.Is(Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(baseUri.Query) ||
                !string.IsNullOrEmpty(baseUri.Fragment) ||
                !string.IsNullOrEmpty(baseUri.UserInfo))
            {
                throw new InvalidOperationException("Flight-status internal API configuration is invalid.");
            }

            var path = $"{baseUri.AbsolutePath.TrimEnd('/')}/{relativePath}";
            return new UriBuilder(baseUri)
            {
                Path = path,
                Query = string.Join("&", query)
            }.Uri;
        }

        private static void AddOptionalQueryParameter(List<string> query, string name, string? value)
        {
            if (value is not null && !value.IsNullOrWhiteSpace())
                query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
        }

        private static string? NormalizeOptional(string? value) =>
            value?.Trim();

    }
}
