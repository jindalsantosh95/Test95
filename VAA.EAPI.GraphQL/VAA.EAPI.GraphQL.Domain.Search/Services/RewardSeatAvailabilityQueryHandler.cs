using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using HotChocolate;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Services
{

    public sealed class RewardSeatAvailabilityQueryHandler : IRewardSeatAvailabilityQueryHandler
    {
        private static readonly string[] Cabins = ["awardEconomy", "awardPremium", "awardUpperClass"];
        private readonly IHttpConnector _http;
        private readonly IDistributedCache _cache;
        private readonly TimeProvider _clock;
        private readonly ILogger<RewardSeatAvailabilityQueryHandler> _log;

        public RewardSeatAvailabilityQueryHandler(
            IHttpConnector http,
            IDistributedCache cache,
            TimeProvider clock,
            ILogger<RewardSeatAvailabilityQueryHandler> log)
        {
            _http = http;
            _cache = cache;
            _clock = clock;
            _log = log;
        }

        public async Task<IEnumerable<RewardSeatAvailabilityMonthResponse>> HandleAsync(
            RewardSeatAvailabilityRequest request, CancellationToken cancellationToken)
        {
            var validation = RewardSeatAvailabilityRequestValidator.Validate(request);
            if (!validation.IsValid)
                throw Error("REWARD_SEAT_INVALID_INPUT", validation.Error!);

            try
            {
                var year = request.Years.Single();
                var requestedMonth = request.Months.Single();
                var horizon = RequiredInt(Constants.Environment.RewardSeatAvailabilityMaxAdvanceDays, 1, 366);
                var maxUnits = RequiredInt(Constants.Environment.RewardSeatAvailabilityMaxWorkUnits, 1, 12);
                var concurrency = RequiredInt(Constants.Environment.RewardSeatAvailabilityMaxConcurrency, 1, 8);
                var timeout = RequiredInt(Constants.Environment.RewardSeatAvailabilityOverallTimeoutSeconds, 1, 120);
                var caps = ReadCaps();
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                deadline.CancelAfter(TimeSpan.FromSeconds(timeout));
                var ct = deadline.Token;
                var instant = _clock.GetUtcNow();
                var deadlineAt = instant.AddSeconds(timeout);
                var (originName, destinationName) = await ValidateRouteAsync(
                    request.Slice.Origin, request.Slice.Destination, "VS", ct);
                var zone = await GetOriginTimeZoneAsync(
                    request.Slice.Origin, request.Slice.Destination, ct);
                var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, zone).DateTime);
                RewardSeatAvailabilityWorkUnit[] workUnits;
                try
                {
                    workUnits = RewardSeatAvailabilityWorkUnitPartitioner
                        .Create(year, requestedMonth, today, horizon, maxUnits)
                        .ToArray();
                }
                catch (ArgumentException)
                {
                    throw Error("REWARD_SEAT_INVALID_INPUT", "The month is outside the booking horizon.");
                }

                var contracts = workUnits.Select(unit => new RewardSeatCalendarContract
                {
                    Origin = request.Slice.Origin,
                    Destination = request.Slice.Destination,
                    AnchorDate = unit.AnchorDate,
                    IntervalStart = unit.IntervalStart,
                    IntervalEnd = unit.IntervalEnd
                }).ToArray();

                using var slots = new SemaphoreSlim(concurrency);
                using var fanout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var workCt = fanout.Token;
                var tasks = contracts.Select(async contract =>
                {
                    await slots.WaitAsync(workCt);
                    try
                    {
                        return await GetCalendarUnitAsync(contract, horizon, caps, deadlineAt, workCt);
                    }
                    catch
                    {
                        fanout.Cancel();
                        throw;
                    }
                    finally
                    {
                        slots.Release();
                    }
                }).ToArray();

                var results = await Task.WhenAll(tasks);
                RewardSeatAvailabilityMonthResponse month;
                try
                {
                    month = RewardSeatAvailabilityMerger.Merge(
                        results, workUnits[0].IntervalStart, workUnits[0].IntervalEnd,
                        requestedMonth, year, originName, destinationName, caps);
                }
                catch (InvalidOperationException)
                {
                    foreach (var contract in contracts)
                    {
                        try
                        {
                            await _cache.RemoveAsync(
                                CalendarKey(contract, horizon, caps,
                                    ConfigReader.Try(Constants.Environment.RewardSeatAvailabilityFareFamilyMappingVersion) ?? string.Empty),
                                ct);
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning(ex, "Reward calendar conflicting cache unit could not be removed.");
                        }
                    }

                    throw;
                }

                return [month];
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Reward calendar request failed.");
                throw Error("REWARD_SEAT_UNAVAILABLE", "Reward seat availability is temporarily unavailable.");
            }
        }

        private IReadOnlyDictionary<string, (int Daily, int Monthly)> ReadCaps()
        {
            var defaults = new[] { ("awardEconomy", 9, 100), ("awardPremium", 9, 50), ("awardUpperClass", 9, 50) };
            return defaults.ToDictionary(x => x.Item1, x =>
            {
                var daily = OptionalInt(Constants.Environment.RewardSeatDailySeatCap(x.Item1), x.Item2);
                var monthly = OptionalInt(Constants.Environment.RewardSeatMonthlySeatCap(x.Item1), x.Item3);
                if (daily < 1 || monthly < 1)
                    throw new InvalidOperationException("Invalid reward calendar seat caps.");

                return (daily, monthly);
            }, StringComparer.Ordinal);
        }

        private int RequiredInt(string key, int min, int max)
        {
            if (!int.TryParse(ConfigReader.Try(key), NumberStyles.None, CultureInfo.InvariantCulture, out var value) ||
                value < min || value > max)
                throw new InvalidOperationException($"Invalid reward calendar setting: {key}.");

            return value;
        }

        private int OptionalInt(string key, int fallback) =>
            ConfigReader.Try(key) is null ? fallback : RequiredInt(key, 1, int.MaxValue);

        private async Task<(string Origin, string Destination)> ValidateRouteAsync(
            string origin, string destination, string carrier, CancellationToken ct)
        {
            var url = ConfigReader.Try(Constants.Environment.AirportInfoRoutesApi);
            if (url.IsNullOrWhiteSpace() || !Uri.TryCreate(url, UriKind.Absolute, out var routeUri) ||
                routeUri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Route feed must use HTTPS.");
            using var routeRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var json = (await _http.SendAsync<JObject>(routeRequest, ct)).Response
                ?? throw new InvalidOperationException("Empty route feed.");
            ValidateRouteSnapshot(json);
            var items = json.SelectToken("$.data.airportList.items") as JArray
                ?? throw new InvalidOperationException("Malformed route feed.");
            JToken? Find(string code) => items.SingleOrDefault(x =>
                x.Value<bool?>("airportEnabled") == true && x.Value<string>("airportCode") == code);

            var from = Find(origin);
            var to = Find(destination);
            if (from is null || to is null || !((from["routes"] as JArray)?.Any(x =>
                    x.Value<string>("toCode") == destination && x.Value<string>("airline") == carrier) ?? false))
                throw Error("REWARD_SEAT_UNSUPPORTED_ROUTE", "This route is not available for reward seat search.");
            var originName = from.Value<string>("name");
            var destinationName = to.Value<string>("name");
            if (originName.IsNullOrWhiteSpace() || destinationName.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Route feed names are incomplete.");

            return (originName, destinationName);
        }

        private static void ValidateRouteSnapshot(JObject value)
        {
            if (value.SelectToken("$.data.airportList.items") is not JArray items || items.Count == 0 ||
                items.Any(item => item is not JObject airport || airport["airportEnabled"]?.Type != JTokenType.Boolean))
                throw new InvalidOperationException("Malformed route feed.");

            // The published feed includes disabled CMS placeholders with no airport code.
            // Only enabled airports participate in route lookup and uniqueness checks.
            var enabled = items.Where(item => item.Value<bool>("airportEnabled")).ToArray();
            if (enabled.Any(airport =>
                    airport.Value<string>("airportCode").IsNullOrWhiteSpace() ||
                    airport.Value<string>("name").IsNullOrWhiteSpace() ||
                    airport["routes"] is not JArray routes ||
                    routes.Any(route => route is not JObject destination ||
                        destination.Value<string>("toCode").IsNullOrWhiteSpace() ||
                        destination.Value<string>("airline").IsNullOrWhiteSpace())) ||
                enabled.GroupBy(item => item.Value<string>("airportCode"), StringComparer.Ordinal)
                    .Any(group => group.Count() > 1))
                throw new InvalidOperationException("Malformed route feed.");
        }

        private async Task<TimeZoneInfo> GetOriginTimeZoneAsync(
            string origin, string destination, CancellationToken ct)
        {
            var template = ConfigReader.Try(Common.Constants.Environment.RefDataMSGenericUrl);
            if (template.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Reference URL is unconfigured.");

            var url = string.Format(CultureInfo.InvariantCulture, template, "airportdetail") +
                $"?airportCodes={origin},{destination}";
            if (!Uri.TryCreate(url, UriKind.Absolute, out var referenceUri) || referenceUri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Reference URL must use HTTPS.");

            using var referenceRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var key = ConfigReader.Try(Common.Constants.Environment.RefDataMSOcpKey);
            if (key.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Reference key is unconfigured.");
            referenceRequest.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", key);
            var airports = (await _http.SendAsync<JArray>(referenceRequest, ct)).Response
                ?? throw new InvalidOperationException("Empty airport metadata.");
            if (airports.Count(x => x.Value<string>("airportCode") == origin) != 1 ||
                airports.Count(x => x.Value<string>("airportCode") == destination) != 1)
                throw new InvalidOperationException("Airport metadata is incomplete.");

            var zoneId = airports.Single(x => x.Value<string>("airportCode") == origin).Value<string>("timeZone");
            if (zoneId.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Origin timezone is missing.");

            return TimeZoneInfoHelper.FindSystemTimeZoneById(zoneId);
        }

        private async Task<RewardSeatCalendarResult> GetCalendarUnitAsync(
            RewardSeatCalendarContract contract, int horizon,
            IReadOnlyDictionary<string, (int Daily, int Monthly)> caps,
            DateTimeOffset deadlineAt, CancellationToken ct)
        {
            var mappingVersion = ConfigReader.Try(Constants.Environment.RewardSeatAvailabilityFareFamilyMappingVersion);
            if (mappingVersion.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Fare family mapping version is missing.");

            var key = CalendarKey(contract, horizon, caps, mappingVersion);
            var ttlText = ConfigReader.Try(Constants.Environment.RewardSeatAvailabilityOfferCacheTtl) ?? "00:15:00";
            if (!TimeSpan.TryParse(ttlText, CultureInfo.InvariantCulture, out var ttl) || ttl < TimeSpan.Zero)
            {
                _log.LogWarning("Reward calendar availability cache TTL is invalid; caching disabled.");
                ttl = TimeSpan.Zero;
            }
            if (ttl > TimeSpan.Zero)
            {
                try
                {
                    var cached = await _cache.GetStringAsync(key, ct);
                    if (cached is not null)
                    {
                        var decoded = Json.Deserialize<RewardSeatCalendarResult>(cached);
                        ValidateUnit(decoded, contract, mappingVersion);
                        _log.LogInformation("Reward calendar work-unit cache hit.");
                        return decoded;
                    }
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    _log.LogWarning("Reward calendar work-unit cache read cancelled by provider; calling Offer.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Reward calendar work-unit cache read failed.");
                }
            }

            _log.LogInformation("Reward calendar work-unit cache miss.");
            var result = await FetchCalendarAsync(contract, deadlineAt, ct);
            ValidateUnit(result, contract, mappingVersion);
            if (ttl > TimeSpan.Zero)
            {
                try
                {
                    await _cache.SetStringAsync(
                        key, Json.Serialize(result),
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    _log.LogWarning("Reward calendar work-unit cache write cancelled by provider.");
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Reward calendar work-unit cache write failed.");
                }
            }

            return result;
        }

        private async Task<RewardSeatCalendarResult> FetchCalendarAsync(
            RewardSeatCalendarContract contract, DateTimeOffset deadlineAt, CancellationToken ct)
        {
            var endpoint = ConfigReader.Try(Constants.Environment.RewardSeatOfferCalendarEndpoint);
            if (endpoint.IsNullOrWhiteSpace() ||
                !Uri.TryCreate(endpoint, UriKind.Absolute, out var calendarUri) ||
                (calendarUri.Scheme != Uri.UriSchemeHttps &&
                    !(calendarUri.Scheme == Uri.UriSchemeHttp && calendarUri.IsLoopback)) ||
                !calendarUri.AbsolutePath.EndsWith("/reward-seat-calendar-availability", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrEmpty(calendarUri.Query) || !string.IsNullOrEmpty(calendarUri.Fragment))
                throw new InvalidOperationException("Offer calendar endpoint is unconfigured.");
            var remaining = Math.Min(30000, (int)(deadlineAt - _clock.GetUtcNow()).TotalMilliseconds);
            if (remaining < 1)
                throw new TimeoutException("Reward calendar deadline expired before Offer execution.");

            using var req = new HttpRequestMessage(HttpMethod.Post, calendarUri);
            var subscriptionKey = ConfigReader.Try(Constants.Environment.SearchOfferMsOcpKey);
            if (!subscriptionKey.IsNullOrWhiteSpace())
                req.Headers.TryAddWithoutValidation(Common.Constants.Application.OcpKeyHeader, subscriptionKey);
            req.Headers.TryAddWithoutValidation(
                "x-reward-seat-deadline-ms", remaining.ToString(CultureInfo.InvariantCulture));
            req.Content = new StringContent(Json.Serialize(contract), Encoding.UTF8, "application/json");
            return (await _http.SendAsync<RewardSeatCalendarResult>(req, ct)).Response
                ?? throw new InvalidOperationException("Empty Offer calendar response.");
        }

        private static void ValidateUnit(
            RewardSeatCalendarResult result, RewardSeatCalendarContract request, string mappingVersion)
        {
            if (result is null || !result.Complete || result.Candidates is null ||
                result.FareFamilyMappingVersion != mappingVersion ||
                result.Candidates.Any(x =>
                    x.Date < request.IntervalStart ||
                    x.Date > request.IntervalEnd ||
                    x.Currency.IsNullOrWhiteSpace() ||
                    x.InventoryCount is < 1 or > 9 ||
                    x.AwardPointsTotal < 0 ||
                    x.CashTotal < 0 ||
                    !HasExpectedIdentity(x, request) ||
                    !Cabins.Contains(x.Cabin, StringComparer.Ordinal)))
                throw new InvalidOperationException("Invalid Offer calendar projection.");

            var solutions = new Dictionary<string, RewardSeatCalendarCandidate>(StringComparer.Ordinal);
            var flights = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var candidate in result.Candidates)
            {
                var identity = string.Join('|',
                    candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    candidate.Cabin, candidate.FlightIdentity, candidate.FareIdentity);
                if (solutions.TryGetValue(identity, out var prior) &&
                    (prior.InventoryCount != candidate.InventoryCount ||
                     prior.AwardPointsTotal != candidate.AwardPointsTotal ||
                     prior.CashTotal != candidate.CashTotal ||
                     prior.Currency != candidate.Currency ||
                     prior.IsSaverAward != candidate.IsSaverAward))
                    throw new InvalidOperationException("Conflicting Offer calendar solution.");

                solutions[identity] = candidate;
                var flight = string.Join('|',
                    candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    candidate.Cabin, candidate.FlightIdentity);
                if (flights.TryGetValue(flight, out var count) && count != candidate.InventoryCount)
                    throw new InvalidOperationException("Conflicting Offer calendar inventory.");

                flights[flight] = candidate.InventoryCount;
            }
        }

        internal static bool HasExpectedIdentity(RewardSeatCalendarCandidate candidate, RewardSeatCalendarContract request)
        {
            var flight = candidate.FlightIdentity?.Split('|');
            return flight is { Length: 8 } &&
                   flight[0] == "VS" &&
                   flight[4] == request.Origin &&
                   flight[5] == request.Destination &&
                   flight[6] == candidate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) &&
                   int.TryParse(flight[1], NumberStyles.None, CultureInfo.InvariantCulture, out var number) &&
                   number > 0 &&
                   int.TryParse(flight[3], NumberStyles.None, CultureInfo.InvariantCulture, out var operatingNumber) &&
                   operatingNumber > 0 &&
                   !flight[2].IsNullOrWhiteSpace() &&
                   !flight[7].IsNullOrWhiteSpace() &&
                   !candidate.FareIdentity.IsNullOrWhiteSpace();
        }

        private static string Hash(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

        private string CalendarKey(
            RewardSeatCalendarContract contract, int horizon,
            IReadOnlyDictionary<string, (int Daily, int Monthly)> caps, string mappingVersion)
        {
            var capPolicy = string.Join(";", caps.OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}:{x.Value.Daily}:{x.Value.Monthly}"));
            var providerVersion = ConfigReader.Try(Constants.Environment.RewardSeatAvailabilityItaVersion) ?? "161";
            var material = Json.Serialize(new
            {
                contract = new
                {
                    contract.Origin,
                    contract.Destination,
                    contract.AnchorDate,
                    contract.IntervalStart,
                    contract.IntervalEnd
                },
                horizon,
                capPolicy,
                mappingVersion,
                providerVersion,
                responseVersion = 2
            });
            return "reward-seat:offer:v4:" + Hash(material);
        }

        private static GraphQLException Error(string code, string message) =>
            new(ErrorBuilder.New().SetCode(code).SetMessage(message).Build());
    }
}
