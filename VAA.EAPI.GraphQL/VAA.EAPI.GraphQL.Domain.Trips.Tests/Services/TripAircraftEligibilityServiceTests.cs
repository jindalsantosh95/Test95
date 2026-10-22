using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using OperationalAircraft = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule.Aircraft;
using TripAircraft = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Aircraft;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services;

[Collection("Flight Ops configuration")]
public sealed class TripAircraftEligibilityServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EnrichAircraftDetailsAsync_WhenScheduleHasNoTailNumber_DoesNotCallStarlink()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(_ => new OperationalScheduleResponse
        {
            Flights = new[] { new Flight { Aircraft = new OperationalAircraft() } }
        });
        var starlinkProvider = new FakeStarlinkEligibilityProvider();
        var leg = CreateLeg("351");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            CreateBooking(CreateSegment("7", leg)), new(true, true));

        Assert.Empty(starlinkProvider.Requests);
        Assert.Null(leg.Aircraft.TailNumber);
        Assert.Null(leg.StarlinkEligible);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_MixedEligibleAndIneligibleSegments_EnrichesOnlyEligibleSegment()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(_ => CreateSchedule("G-VTOM"));
        var starlinkProvider = new FakeStarlinkEligibilityProvider(new Dictionary<string, bool?> { ["G-VTOM"] = true });
        var eligibleLeg = CreateLeg("351");
        var nonVsLeg = CreateLeg("339");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            CreateBooking(
                CreateSegment("7", eligibleLeg, "VS"),
                CreateSegment("8", nonVsLeg, "BA")), new(true, true));

        Assert.Single(scheduleProvider.Requests);
        Assert.Equal(new[] { "G-VTOM" }, Assert.Single(starlinkProvider.Requests));
        Assert.Equal("G-VTOM", eligibleLeg.Aircraft.TailNumber);
        Assert.True(eligibleLeg.StarlinkEligible);
        Assert.Null(nonVsLeg.Aircraft.TailNumber);
        Assert.Null(nonVsLeg.StarlinkEligible);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_CancelledLegInSegment_SkipsCancelledLegAndEnrichesActiveLeg()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(_ => CreateSchedule("G-VTOM"));
        var starlinkProvider = new FakeStarlinkEligibilityProvider(new Dictionary<string, bool?> { ["G-VTOM"] = true });
        var cancelledLeg = CreateLeg("351", "CANCELLED");
        var activeLeg = CreateLeg("351");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            CreateBooking(CreateSegment("7", cancelledLeg, "VS", activeLeg)), new(true, true));

        Assert.Single(scheduleProvider.Requests);
        Assert.Equal(new[] { "G-VTOM" }, Assert.Single(starlinkProvider.Requests));
        Assert.Null(cancelledLeg.Aircraft.TailNumber);
        Assert.Null(cancelledLeg.StarlinkEligible);
        Assert.Equal("G-VTOM", activeLeg.Aircraft.TailNumber);
        Assert.True(activeLeg.StarlinkEligible);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_MultipleEligibleLegs_UsesOneStarlinkBatchContainingAllTailNumbers()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(request =>
            CreateSchedule(request.FlightNumber == 7 ? "G-VTOM" : "G-VPOP"));
        var starlinkProvider = new FakeStarlinkEligibilityProvider(new Dictionary<string, bool?>
        {
            ["G-VTOM"] = true,
            ["G-VPOP"] = false
        });
        var firstLeg = CreateLeg("351");
        var secondLeg = CreateLeg("339");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            CreateBooking(
                CreateSegment("7", firstLeg),
                CreateSegment("8", secondLeg)), new(true, true));

        Assert.Equal(new[] { "G-VTOM", "G-VPOP" }, Assert.Single(starlinkProvider.Requests));
        Assert.True(firstLeg.StarlinkEligible);
        Assert.False(secondLeg.StarlinkEligible);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_MultipleBookings_UsesOneStarlinkBatchAcrossBookings()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(request =>
            CreateSchedule(request.FlightNumber == 7 ? "G-VTOM" : "G-VPOP"));
        var starlinkProvider = new FakeStarlinkEligibilityProvider(new Dictionary<string, bool?>
        {
            ["G-VTOM"] = true,
            ["G-VPOP"] = false
        });
        var firstLeg = CreateLeg("351");
        var secondLeg = CreateLeg("339");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            new[]
            {
                CreateBooking(CreateSegment("7", firstLeg)),
                CreateBooking(CreateSegment("8", secondLeg))
            },
            new(true, true));

        Assert.Equal(new[] { "G-VTOM", "G-VPOP" }, Assert.Single(starlinkProvider.Requests));
        Assert.True(firstLeg.StarlinkEligible);
        Assert.False(secondLeg.StarlinkEligible);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_DuplicateSegments_UsesOneOperationalScheduleLookup()
    {
        var scheduleProvider = new FakeOperationalScheduleProvider(_ => CreateSchedule("G-VTOM"));
        var starlinkProvider = new FakeStarlinkEligibilityProvider(new Dictionary<string, bool?> { ["G-VTOM"] = true });
        var firstLeg = CreateLeg("351");
        var secondLeg = CreateLeg("351");

        await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
            CreateBooking(
                CreateSegment("7", firstLeg),
                CreateSegment("7", secondLeg)),
            new(true, true));

        Assert.Single(scheduleProvider.Requests);
        Assert.Equal("G-VTOM", firstLeg.Aircraft.TailNumber);
        Assert.Equal("G-VTOM", secondLeg.Aircraft.TailNumber);
    }

    [Fact]
    public async Task EnrichAircraftDetailsAsync_LimitsConcurrentOperationalScheduleLookups()
    {
        var originalLimit = ConfigReader.Try(Constants.Environment.FlightOpsMaxConcurrentScheduleRequests, 8);
        ConfigReader.Set(Constants.Environment.FlightOpsMaxConcurrentScheduleRequests, "2");

        var scheduleProvider = new TrackingOperationalScheduleProvider();
        var starlinkProvider = new FakeStarlinkEligibilityProvider();
        var segments = Enumerable.Range(1, 16)
            .Select(flightNumber => CreateSegment(flightNumber.ToString(), CreateLeg(flightNumber.ToString())))
            .ToArray();

        try
        {
            await CreateService(scheduleProvider, starlinkProvider).EnrichAircraftDetailsAsync(
                CreateBooking(segments), new(true, false));

            Assert.Equal(16, scheduleProvider.RequestCount);
            Assert.InRange(scheduleProvider.MaximumConcurrentRequests, 1, 2);
        }
        finally
        {
            ConfigReader.Set(Constants.Environment.FlightOpsMaxConcurrentScheduleRequests, originalLimit.ToString());
        }
    }

    private static TripAircraftEligibilityService CreateService(
        IOperationalScheduleProvider scheduleProvider,
        IStarlinkEligibilityProvider starlinkProvider) =>
        new(scheduleProvider, starlinkProvider, NullLogger<TripAircraftEligibilityService>.Instance,
            new VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers.TestDateTimeProvider(UtcNow));

    private sealed class FakeOperationalScheduleProvider(
        Func<OperationScheduleRequest, OperationalScheduleResponse> responseFactory) : IOperationalScheduleProvider
    {
        public List<OperationScheduleRequest> Requests { get; } = [];

        public Task<OperationalScheduleResponse> GetOperationalScheduleAsync(OperationScheduleRequest request)
        {
            Requests.Add(request);
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class TrackingOperationalScheduleProvider : IOperationalScheduleProvider
    {
        private int _activeRequests;
        private int _requestCount;
        private int _maximumConcurrentRequests;

        public int RequestCount => _requestCount;
        public int MaximumConcurrentRequests => _maximumConcurrentRequests;

        public async Task<OperationalScheduleResponse> GetOperationalScheduleAsync(OperationScheduleRequest request)
        {
            Interlocked.Increment(ref _requestCount);
            var activeRequests = Interlocked.Increment(ref _activeRequests);
            InterlockedMax(ref _maximumConcurrentRequests, activeRequests);

            await Task.Delay(10);

            Interlocked.Decrement(ref _activeRequests);
            return CreateSchedule($"G-{request.FlightNumber}");
        }

        private static void InterlockedMax(ref int target, int value)
        {
            while (true)
            {
                var current = Volatile.Read(ref target);
                if (current >= value || Interlocked.CompareExchange(ref target, value, current) == current) return;
            }
        }
    }

    private sealed class FakeStarlinkEligibilityProvider(
        IReadOnlyDictionary<string, bool?>? eligibility = null) : IStarlinkEligibilityProvider
    {
        private readonly IReadOnlyDictionary<string, bool?> _eligibility = eligibility ?? new Dictionary<string, bool?>();

        public List<string[]> Requests { get; } = [];

        public Task<IReadOnlyDictionary<string, bool?>> GetEligibilityAsync(IEnumerable<string> tailNumbers)
        {
            Requests.Add(tailNumbers.ToArray());
            return Task.FromResult(_eligibility);
        }
    }

    private static Booking CreateBooking(params Segment[] segments) => new()
    {
        Product = new[]
        {
            new Product
            {
                Slices = new[] { new Slice { Segments = segments } }
            }
        }
    };

    private static Segment CreateSegment(string flightNumber, Leg leg, string carrier = "VS", params Leg[] additionalLegs) => new()
    {
        Id = $"segment-{flightNumber}",
        OperatingCarrier = new CarrierDetails { CarrierCode = carrier, FlightNumber = flightNumber },
        ScheduledDepartureUtcDateTime = UtcNow.AddHours(24),
        ScheduledDepartureLocalDateTime = UtcNow.AddHours(24),
        Legs = new[] { leg }.Concat(additionalLegs)
    };

    private static Leg CreateLeg(string aircraftCode, string status = "ON_TIME") => new()
    {
        Aircraft = new TripAircraft { Code = aircraftCode },
        Status = status
    };

    private static OperationalScheduleResponse CreateSchedule(string registration) => new()
    {
        Flights = new[] { new Flight { Aircraft = new OperationalAircraft { Registration = registration } } }
    };
}
