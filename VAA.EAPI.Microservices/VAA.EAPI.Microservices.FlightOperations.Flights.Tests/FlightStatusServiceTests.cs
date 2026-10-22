using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Service;
using Xunit;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Tests;

public sealed class FlightStatusServiceTests
{
    private static readonly DateOnly FlightDate = new(2026, 10, 14);
    private const string BaseUrl = "https://flight-status.test";

    [Fact]
    public async Task RouteSearch_ReturnsMappedRouteAndSendsRequiredHeader()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .RouteSearchAsync("LHR", "JFK", FlightDate, "1", "0", false, TestContext.Current.CancellationToken);

        Assert.Equal(Status.OK, result.Value.Status);
        var route = Assert.Single(result.Value.Value!.Routes!);
        var segment = Assert.Single(route!.Segments!);
        var leg = Assert.Single(segment!.Legs!);
        Assert.NotNull(leg);
        Assert.Equal("leg-1", leg.Id);

        var request = Assert.Single(services.GetRequiredService<TestHttpClientFactory>()
            .GetResponses(BaseUrl)).RequestMessage!;
        Assert.Equal("https://flight-status.test/flights/routes?departureAirportCode=LHR&arrivalAirportCode=JFK&scheduledDepartureDate=2026-10-14&timeRange=1&stops=0&first=false", request.RequestUri!.ToString());
        Assert.Equal("1", Assert.Single(request.Headers.GetValues("x-vaa-flight-status")));
    }

    [Fact]
    public async Task RouteSearch_ReturnsNotFoundForEmptyRoutes()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .RouteSearchAsync("LHR", "BOS", FlightDate, "1", "0", false, TestContext.Current.CancellationToken);

        Assert.Equal(Status.NotFound, result.Value.Status);
        Assert.Equal("FLIGHT_STATUS_NOT_FOUND", Assert.Single(result.Value.Errors!).Code);
        Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task RouteSearch_ReturnsNotFoundForNoContentResponse()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .RouteSearchAsync("LHR", "ATL", FlightDate, "1", "0", false, TestContext.Current.CancellationToken);

        Assert.Equal(Status.NotFound, result.Value.Status);
        Assert.Equal("FLIGHT_STATUS_NOT_FOUND", Assert.Single(result.Value.Errors!).Code);
        Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberSearch_ReturnsCoreDetailsWithOptionalEnrichmentWarnings()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .NumberSearchAsync("leg-1", "VS3", FlightDate, "LHR", TestContext.Current.CancellationToken);

        Assert.Equal(Status.OK, result.Value.Status);
        Assert.Equal("leg-1", result.Value.Value!.ActiveLeg!.Id);
        Assert.Equal("VS3", result.Value.Value.ActiveLeg.MarketingFlightNumber);
        Assert.True(result.Value.Value.ActiveLeg.Detail!.Performance!.OnTime);
        Assert.Single(result.Value.Value.ActiveLeg.Detail.Amenities!);
        Assert.Single(result.Value.Value.ActiveLeg.Detail.Meals!);
        Assert.Null(result.Value.Value.ActiveLeg.Detail.Position);
        Assert.Equal("POSITION_UNAVAILABLE", Assert.Single(result.Value.Value.Warnings).Code);
        var requests = services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl);
        Assert.Equal(4, requests.Count);
        Assert.All(requests, response => Assert.Equal("1", Assert.Single(response.RequestMessage!.Headers.GetValues("x-vaa-flight-status"))));
    }

    [Fact]
    public async Task NumberSearch_WarnsWhenMissingScheduledTimePreventsEnrichment()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .NumberSearchAsync(null, "VS5", FlightDate, "LHR", TestContext.Current.CancellationToken);

        Assert.Equal(Status.OK, result.Value.Status);
        var response = result.Value.Value!;
        Assert.Equal("leg-5", response.ActiveLeg!.Id);
        Assert.Null(response.ActiveLeg.Detail!.Performance);
        Assert.Null(response.ActiveLeg.Detail.Position);
        Assert.Null(response.ActiveLeg.Detail.Amenities);
        Assert.Null(response.ActiveLeg.Detail.Meals);
        Assert.Equal(
            ["FLIGHT_STATUS_STATS_UNAVAILABLE", "POSITION_UNAVAILABLE", "AMENITIES_MEALS_UNAVAILABLE"],
            response.Warnings.Select(warning => warning.Code));
        Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberSearch_ReturnsNotFoundForEmptyBody()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .NumberSearchAsync(null, "VS4", FlightDate, "LHR", TestContext.Current.CancellationToken);

        Assert.Equal(Status.NotFound, result.Value.Status);
        Assert.Equal("FLIGHT_STATUS_NOT_FOUND", Assert.Single(result.Value.Errors!).Code);
        Assert.Single(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberSearch_RejectsMissingDepartureAirportBeforeHttp()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .NumberSearchAsync(null, "VS3", FlightDate, null, TestContext.Current.CancellationToken);

        Assert.Equal(Status.BadRequest, result.Value.Status);
        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(result.Value.Errors!).Code);
        Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task RouteSearch_RejectsInvalidAirportsBeforeHttp()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<IFlightStatusService>()
            .RouteSearchAsync("LH", "JFK", FlightDate, "1", "0", false, TestContext.Current.CancellationToken);

        Assert.Equal(Status.BadRequest, result.Value.Status);
        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(result.Value.Errors!).Code);
        Assert.Empty(services.GetRequiredService<TestHttpClientFactory>().GetResponses(BaseUrl));
    }

    private static ServiceProvider CreateServices()
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
        ConfigReader.Set("FlightStatus:DeltaServiceLayerBaseUrl", BaseUrl);
        ConfigReader.Set("FlightStatus:MaxRetries", "0");
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<TimeProvider>(new FixedTimeProvider())
            .AddSingleton<TestHttpClientFactory>()
            .AddSingleton<IHttpClientFactory>(provider => provider.GetRequiredService<TestHttpClientFactory>())
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IFlightStatusService, FlightStatusService>()
            .BuildServiceProvider();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
    }
}
