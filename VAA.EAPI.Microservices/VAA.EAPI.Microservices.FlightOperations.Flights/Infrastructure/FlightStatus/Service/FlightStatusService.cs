using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Service
{
    public sealed class FlightStatusService(
        IHttpConnector httpConnector,
        ILogger<FlightStatusService> logger,
        TimeProvider? timeProvider = null) : IFlightStatusService
    {
        private const string DefaultDeltaServiceLayerBaseUrl = "https://www.virginatlantic.com";

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<FlightStatusService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
        private readonly string _deltaServiceLayerBaseUrl = BuildDeltaServiceLayerBaseUrl(ConfigReader.Try(Constants.Environment.FlightStatusDeltaServiceLayerBaseUrl, DefaultDeltaServiceLayerBaseUrl));
        private readonly int _maxRetries = Math.Clamp(ConfigReader.Try("FlightStatus:MaxRetries", 2), 0, 2);
        private readonly int _maxResponseBytes = Math.Clamp(ConfigReader.Try("FlightStatus:MaxResponseBytes", 4 * 1024 * 1024), 1024, 16 * 1024 * 1024);
        private readonly SemaphoreSlim _downstreamRequestSemaphore = new(
            Math.Clamp(ConfigReader.Try("FlightStatus:MaxConcurrentDownstreamRequests", 8), 1, 128));

        public async Task<Result<Response<FlightStatusRouteResponse>>> RouteSearchAsync(
            string departureAirportCode,
            string arrivalAirportCode,
            DateOnly scheduledDepartureDate,
            string? timeRange,
            string? stops,
            bool first,
            CancellationToken cancellationToken = default)
        {
            var validationError = ValidateRoute(
                departureAirportCode,
                arrivalAirportCode,
                scheduledDepartureDate,
                timeRange,
                stops);
            if (validationError is not null)
                return Result.Success(BadRequest<FlightStatusRouteResponse>(validationError));

            try
            {
                using var requestDeadline = CreateRequestDeadline(cancellationToken);
                var deltaResponse = await GetDownstreamAsync<DeltaRouteSearchResponse>(
                    BuildRoutesUri(departureAirportCode, arrivalAirportCode, scheduledDepartureDate, timeRange, stops, first),
                    BuildDeltaHeaders(),
                    requestDeadline.Token);
                if (deltaResponse is null)
                    return Result.Success(NotFound<FlightStatusRouteResponse>());

                var downstreamRoutes = deltaResponse.Routes?.Where(route => route is not null).ToArray() ?? [];
                deltaResponse.Routes = downstreamRoutes;
                FlightStatusRouteResponse response = (FlightStatusRouteResponse)deltaResponse;
                var routes = response.Routes?.Where(route => route is not null).Select(route => route!).ToArray() ?? [];
                if (routes.Length == 0)
                {
                    return Result.Success(downstreamRoutes.Length == 0
                        ? NotFound<FlightStatusRouteResponse>()
                        : BadGateway<FlightStatusRouteResponse>());
                }

                if (!IsAirportCode(response.DepartureAirport?.Code) ||
                    !IsAirportCode(response.ArrivalAirport?.Code) ||
                    response.DepartureDate.IsNullOrWhiteSpace())
                {
                    throw new InvalidDataException("Flight status route response is missing required route data.");
                }

                var usableRoutes = new List<FlightStatusRoute>();
                foreach (var route in routes)
                {
                    var usableSegments = route.Segments?
                        .Where(segment => segment is not null)
                        .Select(segment =>
                        {
                            var mappedSegment = segment!;
                            mappedSegment.Legs = mappedSegment.Legs?.Where(IsUsableRouteLeg).ToArray();
                            mappedSegment.LegsWithChangeOfGauge = mappedSegment.LegsWithChangeOfGauge?.Where(IsUsableRouteLeg).ToArray();
                            return mappedSegment;
                        })
                        .Where(segment => segment.Legs?.Any() == true || segment.LegsWithChangeOfGauge?.Any() == true)
                        .ToArray() ?? [];

                    if (usableSegments.Length == 0)
                        continue;

                    route.Segments = usableSegments;
                    usableRoutes.Add(route);
                }

                if (usableRoutes.Count == 0)
                    throw new InvalidDataException("Flight status route response contains no usable legs.");

                response.Routes = usableRoutes;

                return Result.Success(Ok(response, ignoreNullValues: false));
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                _logger.LogWarning("Flight status routes returned downstream HTTP {StatusCode}", (int)ex.StatusCode.Value);
                return Result.Success(DownstreamFailure<FlightStatusRouteResponse>(ex.StatusCode.Value));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Flight status routes downstream request timed out");
                return Result.Success(GatewayTimeout<FlightStatusRouteResponse>());
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Flight status routes downstream request timed out");
                return Result.Success(GatewayTimeout<FlightStatusRouteResponse>());
            }
            catch (Exception)
            {
                _logger.LogWarning("Flight status routes downstream request or mapping failed");
                return Result.Success(BadGateway<FlightStatusRouteResponse>());
            }
        }

        public async Task<Result<Response<FlightStatusNumberResponse>>> NumberSearchAsync(
            string? selectedLegId,
            string flightNumber,
            DateOnly departureDate,
            string? departureAirportCode,
            CancellationToken cancellationToken = default)
        {
            selectedLegId = selectedLegId?.Trim();
            var validationError = ValidateNumber(
                selectedLegId,
                flightNumber,
                departureDate,
                departureAirportCode);
            if (validationError is not null)
                return Result.Success(BadRequest<FlightStatusNumberResponse>(validationError));

            try
            {
                using var requestDeadline = CreateRequestDeadline(cancellationToken);
                var response = await GetNumberAsync(selectedLegId, flightNumber, departureDate, departureAirportCode, BuildDeltaHeaders(), requestDeadline.Token);
                if (response is null)
                    return Result.Success(NotFound<FlightStatusNumberResponse>());

                var mappedLegs = response.Legs?.ToArray() ?? [];
                var usableLegs = mappedLegs.Where(IsUsableNumberLeg).ToArray();
                if (mappedLegs.Length > 0 && usableLegs.Length == 0)
                    throw new InvalidDataException("Flight status details response contains no usable legs.");

                response.Legs = usableLegs;
                response.ActiveLeg = selectedLegId is null
                    ? usableLegs.FirstOrDefault()
                    : usableLegs.FirstOrDefault(leg => leg.Id.Is(selectedLegId))
                      ?? usableLegs.FirstOrDefault();

                if (response.ActiveLeg is null && (response.Legs is null || !response.Legs.Any()))
                    return Result.Success(NotFound<FlightStatusNumberResponse>());

                if (response.ActiveLeg is not null)
                {
                    var activeLeg = response.ActiveLeg;
                    var resolvedDepartureAirportCode = activeLeg.Departure?.AirportCode ?? departureAirportCode;
                    var resolvedArrivalAirportCode = activeLeg.Arrival?.AirportCode;
                    var scheduledDepartureTime = activeLeg.Departure?.ScheduledTime;

                    var detail = new FlightStatusLegDetail
                    {
                        Aircraft = activeLeg.Aircraft,
                        Status = activeLeg.Status
                    };

                    var warnings = new List<FlightStatusWarning>();

                    if (HasEnrichmentInputs(resolvedDepartureAirportCode, resolvedArrivalAirportCode, scheduledDepartureTime))
                    {
                        using var enrichmentDeadline = CreateEnrichmentDeadline(requestDeadline.Token);
                        var performanceTask = TryGetEnrichmentAsync(
                            token => GetPerformanceAsync(flightNumber, resolvedDepartureAirportCode, resolvedArrivalAirportCode, scheduledDepartureTime, BuildDeltaHeaders(), token),
                            FlightStatusWarningCodes.StatisticsUnavailable,
                            cancellationToken,
                            enrichmentDeadline.Token);
                        var positionTask = TryGetEnrichmentAsync(
                            token => GetPositionAsync(flightNumber, resolvedDepartureAirportCode, resolvedArrivalAirportCode, scheduledDepartureTime, BuildDeltaHeaders(), token),
                            FlightStatusWarningCodes.PositionUnavailable,
                            cancellationToken,
                            enrichmentDeadline.Token);
                        var amenitiesAndMealsTask = TryGetEnrichmentAsync(
                            token => GetAmenitiesAndMealsAsync(flightNumber, resolvedDepartureAirportCode, resolvedArrivalAirportCode, scheduledDepartureTime, BuildDeltaHeaders(), token),
                            FlightStatusWarningCodes.AmenitiesMealsUnavailable,
                            cancellationToken,
                            enrichmentDeadline.Token);

                        await Task.WhenAll(performanceTask, positionTask, amenitiesAndMealsTask);

                        var performance = await performanceTask;
                        var position = await positionTask;
                        var amenitiesAndMeals = await amenitiesAndMealsTask;

                        detail.Performance = performance.Value;
                        detail.Position = position.Value;
                        detail.Amenities = amenitiesAndMeals.Value?.Amenities;
                        detail.Meals = amenitiesAndMeals.Value?.Meals;

                        AddWarning(warnings, performance.WarningCode, "Flight statistics are unavailable.");
                        AddWarning(warnings, position.WarningCode);
                        AddWarning(warnings, amenitiesAndMeals.WarningCode);
                    }
                    else
                    {
                        AddWarning(warnings, FlightStatusWarningCodes.StatisticsUnavailable, "Flight statistics are unavailable.");
                        AddWarning(warnings, FlightStatusWarningCodes.PositionUnavailable);
                        AddWarning(warnings, FlightStatusWarningCodes.AmenitiesMealsUnavailable);
                    }

                    LogEnrichmentWarnings(warnings);

                    activeLeg.Detail = detail;
                    response.Warnings = warnings;
                }

                return Result.Success(Ok(response));
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                _logger.LogWarning("Flight status details returned downstream HTTP {StatusCode}", (int)ex.StatusCode.Value);
                return Result.Success(DownstreamFailure<FlightStatusNumberResponse>(ex.StatusCode.Value));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Flight status details downstream request timed out");
                return Result.Success(GatewayTimeout<FlightStatusNumberResponse>());
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Flight status details downstream request timed out");
                return Result.Success(GatewayTimeout<FlightStatusNumberResponse>());
            }
            catch (Exception)
            {
                _logger.LogWarning("Flight status details downstream request or mapping failed");
                return Result.Success(BadGateway<FlightStatusNumberResponse>());
            }
        }

        private async Task<FlightStatusNumberResponse?> GetNumberAsync(string? selectedLegId, string flightNumber, DateOnly departureDate, string? departureAirportCode, Dictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var deltaResponse = await GetDownstreamAsync<DeltaFlightDetailsResponse>(BuildDetailsUri(flightNumber, departureDate, departureAirportCode), headers, cancellationToken);
            if (deltaResponse is null)
                return null;

            deltaResponse.SelectedLegId = selectedLegId;
            deltaResponse.FlightNumber = flightNumber;

            var response = (FlightStatusNumberResponse)deltaResponse;
            if (response.FlightDate.IsNullOrWhiteSpace())
                response.FlightDate = departureDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return response;
        }

        private async Task<FlightStatusPerformance?> GetPerformanceAsync(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime, Dictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var stats = await GetDownstreamAsync<JObject>(BuildStatsUri(flightNumber, departureAirportCode, arrivalAirportCode, scheduledDepartureTime), headers, cancellationToken);
            if (stats is null)
                return null;

            var performance = (FlightStatusPerformance)stats;
            return performance.MilesFlown is null &&
                   performance.OnTime is null &&
                   performance.ArrivalDelay30 is null &&
                   performance.ArrivalDelay60 is null &&
                   performance.Cancellation is null
                ? null
                : performance;
        }

        private async Task<FlightStatusPosition?> GetPositionAsync(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime, Dictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var position = await GetDownstreamAsync<DeltaFlightPositionResponse>(BuildPositionUri(flightNumber, departureAirportCode, arrivalAirportCode, scheduledDepartureTime), headers, cancellationToken);

            return (FlightStatusPosition?)position;
        }

        private async Task<FlightStatusAmenitiesAndMeals?> GetAmenitiesAndMealsAsync(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime, Dictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var response = await GetDownstreamAsync<DeltaAmenitiesAndMealsResponse>(BuildAmenitiesAndMealsUri(flightNumber, departureAirportCode, arrivalAirportCode, scheduledDepartureTime), headers, cancellationToken);
            if (response is null)
                return null;

            var amenities = response.Amenities?
                .Where(item => item is not null)
                .Select(item => (FlightStatusAmenity?)item)
                .ToArray();
            var meals = response.Meals?
                .Where(item => item is not null)
                .Select(item => (FlightStatusMeal?)item)
                .ToArray();

            if (amenities is not { Length: > 0 } && meals is not { Length: > 0 })
                return null;

            return new FlightStatusAmenitiesAndMeals(
                amenities is { Length: > 0 } ? amenities : null,
                meals is { Length: > 0 } ? meals : null);
        }

        private async Task<T?> GetDownstreamAsync<T>(string uri, Dictionary<string, string> headers, CancellationToken cancellationToken)
            where T : class
        {
            for (var attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _downstreamRequestSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                        request.Headers.TryAddWithoutValidation("Accept", "application/json");
                        request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
                        request.Headers.TryAddWithoutValidation("x-nowait", "true");
                        foreach (var header in headers)
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        request.SuppressRequestUriLogging();
                        using var response = await _httpConnector.SendAsync(request, cancellationToken);

                        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.RedirectMethod)
                        {
                            var failure = new HttpRequestException(
                                "Flight-status dependency request failed.",
                                null,
                                response.StatusCode);
                            AddRetryAfter(failure, response);
                            throw failure;
                        }

                        if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.RedirectMethod)
                            return default;

                        var content = await ReadContentAsync(response, _maxResponseBytes, cancellationToken);
                        return content.Length == 0
                            ? default
                            : Json.Deserialize<T>(Encoding.UTF8.GetString(content));
                    }
                    finally
                    {
                        _downstreamRequestSemaphore.Release();
                    }
                }
                catch (HttpRequestException exception) when (attempt < _maxRetries && IsTransient(exception))
                {
                    double? retryAfterMilliseconds = exception.Data["RetryAfterMilliseconds"] is double milliseconds
                        ? milliseconds
                        : null;
                    var retryDelay = GetRetryDelay(retryAfterMilliseconds, attempt);
                    _logger.LogInformation(
                        "Retrying Flight Status dependency request after a transient failure. Retry {RetryCount} of {MaxRetries}.",
                        attempt + 1,
                        _maxRetries);
                    await Task.Delay(retryDelay, cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < _maxRetries)
                {
                    var retryDelay = GetRetryDelay(null, attempt);
                    _logger.LogInformation(
                        "Retrying Flight Status dependency request after a transient timeout. Retry {RetryCount} of {MaxRetries}.",
                        attempt + 1,
                        _maxRetries);
                    await Task.Delay(retryDelay, cancellationToken);
                }
            }
        }

        private static void AddRetryAfter(HttpRequestException exception, HttpResponseMessage response)
        {
            var retryAfter = response.Headers.RetryAfter;
            var retryAfterMilliseconds = retryAfter?.Delta?.TotalMilliseconds ??
                (retryAfter?.Date is { } retryAt ? (retryAt - DateTimeOffset.UtcNow).TotalMilliseconds : 0);

            if (retryAfterMilliseconds > 0)
                exception.Data["RetryAfterMilliseconds"] = retryAfterMilliseconds;
        }

        private static async Task<byte[]> ReadContentAsync(
            HttpResponseMessage response,
            int maxResponseBytes,
            CancellationToken cancellationToken)
        {
            if (response.Content.Headers.ContentLength is long contentLength && contentLength > maxResponseBytes)
                throw new InvalidDataException("Flight-status dependency response exceeded the configured size limit.");

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
                    throw new InvalidDataException("Flight-status dependency response exceeded the configured size limit.");

                await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            return output.ToArray();
        }

        private static bool IsTransient(HttpRequestException exception) =>
            exception.StatusCode is null or HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
            exception.StatusCode is { } status && (int)status >= 500;

        private static TimeSpan GetRetryDelay(double? retryAfterMilliseconds, int attempt)
        {
            var maxJitterMilliseconds = 250 * (attempt + 1);
            if (retryAfterMilliseconds is > 0)
            {
                var jitterMilliseconds = Random.Shared.Next(0, maxJitterMilliseconds + 1);
                var maximumSupportedDelayMilliseconds = TimeSpan.FromDays(30).TotalMilliseconds;
                var boundedRetryAfterMilliseconds = Math.Min(
                    retryAfterMilliseconds.Value,
                    maximumSupportedDelayMilliseconds - jitterMilliseconds);
                return TimeSpan.FromMilliseconds(boundedRetryAfterMilliseconds + jitterMilliseconds);
            }

            return TimeSpan.FromMilliseconds(Random.Shared.Next(100, maxJitterMilliseconds + 1));
        }

        private static async Task<OptionalEnrichment<T>> TryGetEnrichmentAsync<T>(Func<CancellationToken, Task<T?>> getEnrichment, string warningCode, CancellationToken callerCancellationToken, CancellationToken enrichmentCancellationToken)
            where T : class
        {
            try
            {
                var value = await getEnrichment(enrichmentCancellationToken);
                return value is null
                    ? new OptionalEnrichment<T>(null, warningCode)
                    : new OptionalEnrichment<T>(value, null);
            }
            catch (OperationCanceledException) when (callerCancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                return new OptionalEnrichment<T>(null, warningCode);
            }
        }

        private static CancellationTokenSource CreateRequestDeadline(CancellationToken cancellationToken) =>
            CreateDeadline(cancellationToken, "FlightStatus:RequestTimeoutSeconds", 20);

        private static CancellationTokenSource CreateEnrichmentDeadline(CancellationToken cancellationToken) =>
            CreateDeadline(cancellationToken, "FlightStatus:EnrichmentDeadlineSeconds", 8);

        private static CancellationTokenSource CreateDeadline(CancellationToken cancellationToken, string settingName, int defaultSeconds)
        {
            var timeoutSeconds = Math.Clamp(ConfigReader.Try(settingName, defaultSeconds), 1, 120);
            var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return deadline;
        }

        private static bool HasEnrichmentInputs(string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime) =>
            !departureAirportCode.IsNullOrWhiteSpace() &&
            !arrivalAirportCode.IsNullOrWhiteSpace() &&
            !scheduledDepartureTime.IsNullOrWhiteSpace();

        private static void AddWarning(ICollection<FlightStatusWarning> warnings, string? warningCode, string? message = null)
        {
            if (warningCode is not null)
                warnings.Add(new FlightStatusWarning { Code = warningCode, Message = message ?? string.Empty });
        }

        private void LogEnrichmentWarnings(IEnumerable<FlightStatusWarning> warnings)
        {
            foreach (var warning in warnings)
                _logger.LogWarning("Flight Status optional enrichment unavailable. WarningCode: {WarningCode}", warning.Code);
        }

        private string BuildRoutesUri(string departureAirportCode, string arrivalAirportCode, DateOnly scheduledDepartureDate, string? timeRange, string? stops, bool first) =>
            $"{_deltaServiceLayerBaseUrl}/flights/routes?departureAirportCode={Encode(departureAirportCode)}&arrivalAirportCode={Encode(arrivalAirportCode)}&scheduledDepartureDate={scheduledDepartureDate:yyyy-MM-dd}&timeRange={Encode(NormalizeFilter(timeRange, "1,2,3"))}&stops={Encode(NormalizeFilter(stops, "0,1,2"))}&first={first.ToString().ToLowerInvariant()}";

        private string BuildDetailsUri(string flightNumber, DateOnly departureDate, string? departureAirportCode)
        {
            return $"{_deltaServiceLayerBaseUrl}/flights/details?flightNumber={Encode(flightNumber)}&departureDate={departureDate:yyyy-MM-dd}&departureAirportCode={Encode(departureAirportCode)}";
        }

        private string BuildStatsUri(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime) =>
            $"{_deltaServiceLayerBaseUrl}/stats/?departureAirportCode={Encode(departureAirportCode)}&arrivalAirportCode={Encode(arrivalAirportCode)}&flightNumber={Encode(flightNumber)}&scheduledDepartureTime={Encode(scheduledDepartureTime)}";

        private string BuildPositionUri(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime) =>
            BuildFlightStatusEnrichmentUri("/flights/positions/", flightNumber, departureAirportCode, arrivalAirportCode, scheduledDepartureTime);

        private string BuildAmenitiesAndMealsUri(string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime) =>
            BuildFlightStatusEnrichmentUri("/flights/amenitiesAndMeals", flightNumber, departureAirportCode, arrivalAirportCode, scheduledDepartureTime);

        private string BuildFlightStatusEnrichmentUri(string resourcePath, string flightNumber, string? departureAirportCode, string? arrivalAirportCode, string? scheduledDepartureTime) =>
            $"{_deltaServiceLayerBaseUrl}{resourcePath}?departureAirportCode={Encode(departureAirportCode)}&arrivalAirportCode={Encode(arrivalAirportCode)}&flightNumber={Encode(flightNumber)}&departureDateTime={Encode(scheduledDepartureTime)}";

        private static Dictionary<string, string> BuildDeltaHeaders() => new(StringComparer.OrdinalIgnoreCase)
        {
            ["x-vaa-flight-status"] = "1"
        };

        private static string BuildDeltaServiceLayerBaseUrl(string configuredBaseUrl)
        {
            var baseUrl = (configuredBaseUrl.IsNullOrWhiteSpace() ? DefaultDeltaServiceLayerBaseUrl : configuredBaseUrl).TrimEnd('/');

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                return DefaultDeltaServiceLayerBaseUrl;

            // Always prefer HTTPS for the production host; allow alternate hosts (e.g. localhost) as configured.
            if (uri.Host.Is("www.virginatlantic.com"))
            {
                var builder = new UriBuilder(uri)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = -1
                };

                return builder.Uri.ToString().TrimEnd('/');
            }

            return uri.ToString().TrimEnd('/');
        }

        private string? ValidateRoute(
            string departureAirportCode,
            string arrivalAirportCode,
            DateOnly scheduledDepartureDate,
            string? timeRange,
            string? stops)
        {
            if (!IsAirportCode(departureAirportCode) || !IsAirportCode(arrivalAirportCode))
                return "Departure and arrival airport codes must be three-letter IATA codes.";

            if (departureAirportCode.Is(arrivalAirportCode))
                return "Departure and arrival airport codes must be different.";

            if (scheduledDepartureDate == default)
                return "Scheduled departure date is required.";

            if (!IsPermittedFlightDate(scheduledDepartureDate))
                return "Flight date must be between 1 day before today and 331 days after today.";

            if (!IsAllowedFilter(timeRange, [1, 2, 3]) || !IsAllowedFilter(stops, [0, 1, 2]))
                return "Route filters are invalid.";

            return null;
        }

        private string? ValidateNumber(
            string? selectedLegId,
            string flightNumber,
            DateOnly departureDate,
            string? departureAirportCode)
        {
            if (selectedLegId is { Length: > 32 } || selectedLegId is not null && selectedLegId.Length == 0)
                return "Selected leg id is invalid.";

            if (flightNumber.IsNullOrWhiteSpace() || flightNumber.Length > 6)
                return "Flight number is required and must be six characters or fewer.";

            if (!flightNumber.All(char.IsAsciiLetterOrDigit))
            {
                return "Flight number is invalid.";
            }

            if (!IsAirportCode(departureAirportCode))
                return "Departure airport code is required and must be a three-letter IATA code.";

            if (departureDate == default)
                return "Departure date is required.";

            return IsPermittedFlightDate(departureDate)
                ? null
                : "Flight date must be between 1 day before today and 331 days after today.";
        }

        private bool IsPermittedFlightDate(DateOnly flightDate)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
            return flightDate >= today.AddDays(-1) && flightDate <= today.AddDays(331);
        }

        private static bool IsAirportCode(string? value) => value?.Length == 3 && value.All(char.IsAsciiLetter);

        private static bool IsUsableRouteLeg(FlightStatusRouteLeg? leg) =>
            leg is not null &&
            !leg.Id.IsNullOrWhiteSpace() &&
            !leg.FlightNumber.IsNullOrWhiteSpace() &&
            IsAirportCode(leg.DepartureAirport?.Code) &&
            IsAirportCode(leg.ArrivalAirport?.Code);

        private static bool IsUsableNumberLeg(FlightStatusLeg leg) =>
            !leg.Id.IsNullOrWhiteSpace() &&
            !leg.MarketingFlightNumber.IsNullOrWhiteSpace() &&
            IsAirportCode(leg.Departure?.AirportCode) &&
            IsAirportCode(leg.Arrival?.AirportCode);

        private static bool IsAllowedFilter(string? value, IReadOnlyCollection<int> allowed)
        {
            if (value.IsNullOrWhiteSpace())
                return true;

            var parts = value!.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length is < 1 or > 3)
                return false;

            var values = new List<int>(parts.Length);
            foreach (var part in parts)
            {
                if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                    return false;

                values.Add(parsed);
            }

            return values.Distinct().Count() == values.Count && values.All(allowed.Contains);
        }

        private static string NormalizeFilter(string? value, string defaultValue) =>
            value.IsNullOrWhiteSpace()
                ? defaultValue
                : string.Join(",", value!.Split(',', StringSplitOptions.TrimEntries));

        private static string Encode(string? value) => HttpUtility.UrlEncode(value ?? string.Empty);

        private static string EncodePathSegment(string? value) => Uri.EscapeDataString(value ?? string.Empty);

        private static Response<T> Ok<T>(T value, bool ignoreNullValues = true) => new()
        {
            Status = Status.OK,
            Value = value,
            IgnoreNullValues = ignoreNullValues
        };

        private static Response<T> BadRequest<T>(string message) => new()
        {
            Status = Status.BadRequest,
            ErrorMessage = message,
            Errors =
            [
                new ErrorDetails
                {
                    Code = "FLIGHT_STATUS_INVALID_INPUT",
                    Message = message
                }
            ]
        };

        private static Response<T> NotFound<T>() =>
            Failure<T>(Status.NotFound, "FLIGHT_STATUS_NOT_FOUND", "Flight status was not found.");

        private static Response<T> DownstreamFailure<T>(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => Failure<T>(Status.BadRequest, "FLIGHT_STATUS_INVALID_INPUT", "Flight status request was rejected."),
            HttpStatusCode.NotFound => Failure<T>(Status.NotFound, "FLIGHT_STATUS_NOT_FOUND", "Flight status was not found."),
            HttpStatusCode.RequestTimeout => GatewayTimeout<T>(),
            HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable => Unavailable<T>(),
            HttpStatusCode.GatewayTimeout => GatewayTimeout<T>(),
            _ => BadGateway<T>()
        };

        private static Response<T> BadGateway<T>() =>
            Failure<T>(Status.BadGateway, "FLIGHT_STATUS_BAD_GATEWAY", "The flight status provider could not complete the request.");

        private static Response<T> Unavailable<T>() =>
            Failure<T>(Status.ServiceUnavailable, "FLIGHT_STATUS_UNAVAILABLE", "Flight status is temporarily unavailable.");

        private static Response<T> GatewayTimeout<T>() =>
            Failure<T>(Status.GatewayTimeout, "FLIGHT_STATUS_TIMEOUT", "The flight status provider did not respond in time.");

        private static Response<T> Failure<T>(Status status, string code, string message) => new()
        {
            Status = status,
            ErrorMessage = message,
            Errors =
            [
                new ErrorDetails
                {
                    Code = code,
                    Message = message
                }
            ]
        };

        private sealed record OptionalEnrichment<T>(T? Value, string? WarningCode) where T : class;

        private sealed record FlightStatusAmenitiesAndMeals(
            IEnumerable<FlightStatusAmenity?>? Amenities,
            IEnumerable<FlightStatusMeal?>? Meals);
    }
}
