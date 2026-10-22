using Microsoft.Extensions.Logging;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using TripMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using TripsEnvironment = VAA.EAPI.GraphQL.Domain.Trips.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services;

public sealed class TripAircraftEligibilityService : ITripAircraftEligibilityService
{
    private readonly IOperationalScheduleProvider _operationalScheduleProvider;
    private readonly IStarlinkEligibilityProvider _starlinkEligibilityProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TripAircraftEligibilityService> _logger;
    private readonly SemaphoreSlim _scheduleRequestConcurrency;

    public TripAircraftEligibilityService(IOperationalScheduleProvider operationalScheduleProvider, IStarlinkEligibilityProvider starlinkEligibilityProvider, ILogger<TripAircraftEligibilityService> logger, IDateTimeProvider dateTimeProvider = null)
    {
        _operationalScheduleProvider = operationalScheduleProvider ?? throw new ArgumentNullException(nameof(operationalScheduleProvider));
        _starlinkEligibilityProvider = starlinkEligibilityProvider ?? throw new ArgumentNullException(nameof(starlinkEligibilityProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();

        var maxConcurrentScheduleRequests = Math.Max(
            1,
            ConfigReader.Try(TripsEnvironment.FlightOpsMaxConcurrentScheduleRequests, TripsEnvironment.DefaultFlightOpsMaxConcurrentScheduleRequests));
        _scheduleRequestConcurrency = new(maxConcurrentScheduleRequests, maxConcurrentScheduleRequests);
    }

    public Task EnrichAircraftDetailsAsync(TripMS.Booking booking, TripAircraftEnrichmentOptions options) =>
        EnrichAircraftDetailsAsync(booking is null ? [] : [booking], options);

    public async Task EnrichAircraftDetailsAsync(IEnumerable<TripMS.Booking> bookings, TripAircraftEnrichmentOptions options)
    {
        if (bookings is null || options is null || !options.RequiresOperationalSchedule) return;

        var segments = bookings.Where(booking => booking is not null)
            .SelectMany(booking => booking.Product ?? [])
            .SelectMany(product => product?.Slices ?? [])
            .SelectMany(slice => slice?.Segments ?? [])
            .Where(x => x?.OperatingCarrier?.CarrierCode.Is("VS") == true
                && IsEligible(x.ScheduledDepartureUtcDateTime)
                && (x.Legs?.Any(l => l?.Status.Is("CANCELLED") != true
                    && l?.Aircraft?.Code.IsNullOrWhiteSpace() == false) ?? false))
            .ToArray() ?? [];

        var lookups = segments
            .Select(segment => (Segment: segment, Request: (OperationScheduleRequest)segment))
            .GroupBy(x => CreateLookupKey(x.Request))
            .Select(group => new
            {
                Segments = group.Select(x => x.Segment).ToArray(),
                ScheduleTask = GetOperationalScheduleSafeAsync(group.First().Segment, group.First().Request)
            })
            .ToArray();
        await Task.WhenAll(lookups.Select(x => x.ScheduleTask));

        var eligibleLegs = new List<TripMS.Leg>();

        foreach (var lookup in lookups)
        {
            var schedule = await lookup.ScheduleTask;

            foreach (var segment in lookup.Segments)
            {
                foreach (var leg in segment.Legs ?? [])
                {
                    var tailNumber = schedule?.Flights?.FirstOrDefault()?.Aircraft?.Registration;
                    if (leg is null
                        || leg.Status.Is("CANCELLED")
                        || leg.Aircraft?.Code.IsNullOrWhiteSpace() == true
                        || tailNumber.IsNullOrWhiteSpace()) continue;

                    leg.Aircraft = (TripMS.Aircraft)(leg.Aircraft, schedule);
                    eligibleLegs.Add(leg);
                }
            }
        }

        if (!options.IncludeStarlinkEligibility || eligibleLegs.Count == 0) return;

        try
        {
            var tailNumbers = eligibleLegs.Select(x => x.Aircraft.TailNumber?.Trim()).Where(x => !x.IsNullOrWhiteSpace()).ToArray();
            var eligibility = await _starlinkEligibilityProvider.GetEligibilityAsync(tailNumbers);

            foreach (var leg in eligibleLegs)
            {
                var tailNumber = leg.Aircraft.TailNumber?.Trim();
                leg.StarlinkEligible = tailNumber is not null && eligibility.TryGetValue(tailNumber, out var value) ? value : null;
            }
        }

        catch (Exception ex)
        {
            var tailNumbers = string.Join(",", eligibleLegs.Select(x => x.Aircraft?.TailNumber).Where(x => !x.IsNullOrWhiteSpace()));
            var aircraftNumbers = string.Join(",", eligibleLegs.Select(x => x.Aircraft?.Code).Where(x => !x.IsNullOrWhiteSpace()));

            _logger.LogWarning(ex, "Starlink eligibility batch lookup failed for {LegCount} legs. Aircraft numbers: {AircraftNumbers}. Tail numbers: {TailNumbers}.", eligibleLegs.Count, aircraftNumbers, tailNumbers);
        }
    }

    private bool IsEligible(DateTime departure) => departure != default && departure > _dateTimeProvider.UtcNow && departure <= _dateTimeProvider.UtcNow.AddHours(72);

    private async Task<OperationalScheduleResponse> GetOperationalScheduleSafeAsync(TripMS.Segment segment, OperationScheduleRequest request)
    {
        await _scheduleRequestConcurrency.WaitAsync();

        try
        {
            return await _operationalScheduleProvider.GetOperationalScheduleAsync(request);
        }

        catch (Exception ex)
        {
            var aircraftNumbers = string.Join(",", segment.Legs?.Where(x => x?.Aircraft?.Code.IsNullOrEmpty() == false).Select(x => x.Aircraft.Code) ?? []);

            _logger.LogWarning(ex, "Aircraft tail-number lookup failed for segment {SegmentId}. Aircraft numbers: {AircraftNumbers}.", segment.Id, aircraftNumbers);
            return null;
        }

        finally
        {
            _scheduleRequestConcurrency.Release();
        }
    }

    private static ScheduleLookupKey CreateLookupKey(OperationScheduleRequest request) => new(
        request.AirlineCode?.Trim().ToUpperInvariant() ?? string.Empty,
        request.FlightNumber,
        request.LocalDateFrom,
        request.LocalDateTo,
        request.Direction?.Trim().ToUpperInvariant() ?? string.Empty,
        request.Limit,
        request.OriginAirportCode?.Trim().ToUpperInvariant() ?? string.Empty,
        request.DestinationAirportCode?.Trim().ToUpperInvariant() ?? string.Empty);

    private readonly record struct ScheduleLookupKey(
        string AirlineCode,
        int FlightNumber,
        LocalDateTime LocalDateFrom,
        LocalDateTime LocalDateTo,
        string Direction,
        int Limit,
        string OriginAirportCode,
        string DestinationAirportCode);

}
