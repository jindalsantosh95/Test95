using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;
using VAA.EAPI.GraphQL.Domain.Operations.Queries;
using VAA.EAPI.GraphQL.Domain.Operations.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Operations.Tests;

public sealed class FlightStatusProviderTests
{
    private static readonly DateOnly FlightDate = new(2026, 10, 14);
    private const string BaseUrl = "https://flight-status-internal.test/api";

    [Fact]
    public async Task RouteQuery_ReturnsRouteAndMakesOneSubscriptionKeyOnlyInternalCall()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByRoute("LHR", "JFK", FlightDate, [1], [0], false, CancellationToken.None);

        Assert.Equal("2026-10-14", result!.DepartureDate);
        Assert.Single(result.Routes!);
        var request = Assert.Single(services.GetRequiredService<HttpClientFactory>()
            .GetResponses(BaseUrl)).RequestMessage!;
        Assert.Equal("https://flight-status-internal.test/api/flightstatus/routes/LHR/JFK/2026-10-14?timeRange=1&stops=0&first=false", request.RequestUri!.ToString());
        Assert.Null(request.Headers.Authorization);
        Assert.Equal("test-subscription-key", Assert.Single(request.Headers.GetValues("Ocp-Apim-Subscription-Key")));
    }

    [Fact]
    public async Task NumberQuery_ReturnsCoreAndEnrichedDetailsFromOneInternalCall()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByNumber("VS3", FlightDate, "LHR", "leg-1", CancellationToken.None);

        Assert.Equal("leg-1", result!.ActiveLeg!.Id);
        Assert.Equal("LHR", result.ActiveLeg.Departure!.AirportCode);
        Assert.Equal("2026-10-14T12:00:00Z", result.ActiveLeg.Detail!.Status!.ScheduledDepartureTime);
        Assert.Equal("A350", result.ActiveLeg.Aircraft!.Type);
        Assert.Equal(334, result.ActiveLeg.Aircraft.Specification!.Range!.InMiles);
        Assert.Equal("WIFI", Assert.IsType<FlightStatusAmenity>(Assert.Single(result.ActiveLeg.Detail.Amenities!)).Code);
        Assert.Equal("VGML", Assert.IsType<FlightStatusMeal>(Assert.Single(result.ActiveLeg.Detail.Meals!)).Code);
        Assert.NotNull(result.ActiveLeg.Detail!.Performance);
        Assert.NotNull(result.ActiveLeg.Detail.Position);
        var request = Assert.Single(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl)).RequestMessage!;
        Assert.Equal("https://flight-status-internal.test/api/flightstatus/flights/VS3/2026-10-14?departureAirportCode=LHR&selectedLegId=leg-1", request.RequestUri!.ToString());
        Assert.Null(request.Headers.Authorization);
        Assert.Equal("test-subscription-key", Assert.Single(request.Headers.GetValues("Ocp-Apim-Subscription-Key")));
    }

    [Fact]
    public async Task RouteQuery_MapsMissingRouteToPublicNotFoundError()
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByRoute("LHR", "BOS", FlightDate, null, null, null, CancellationToken.None));

        Assert.Equal("No results.", Assert.Single(error.Errors).Message);
        Assert.Equal("FLIGHT_STATUS_NOT_FOUND", Assert.Single(error.Errors).Extensions!["code"]);
        Assert.Null(Assert.Single(error.Errors).Exception);
        Assert.Single(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberQuery_RejectsInvalidFlightNumberBeforeHttp()
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByNumber("VS!", FlightDate, "LHR", null, CancellationToken.None));

        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(error.Errors).Extensions!["code"]);
        Assert.Null(Assert.Single(error.Errors).Exception);
        Assert.Empty(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Theory]
    [InlineData("LH1", "JFK")]
    [InlineData("LHR", "LHR")]
    public async Task RouteQuery_RejectsInvalidAirportsBeforeHttp(string departure, string arrival)
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByRoute(departure, arrival, FlightDate, null, null, null, CancellationToken.None));

        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(error.Errors).Extensions!["code"]);
        Assert.Empty(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberQuery_RejectsDateOutsidePermittedWindowBeforeHttp()
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByNumber("VS3", FlightDate.AddDays(332), "LHR", null, CancellationToken.None));

        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(error.Errors).Extensions!["code"]);
        Assert.Empty(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task NumberQuery_RejectsMissingDepartureAirportBeforeHttp()
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByNumber("VS3", FlightDate, null!, null, CancellationToken.None));

        Assert.Equal("FLIGHT_STATUS_INVALID_INPUT", Assert.Single(error.Errors).Extensions!["code"]);
        Assert.Empty(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Theory]
    [InlineData("MIA", "FLIGHT_STATUS_TIMEOUT", "Flight-status service did not respond in time.")]
    [InlineData("ORD", "FLIGHT_STATUS_UNAVAILABLE", "Flight-status service is temporarily unavailable.")]
    public async Task RouteQuery_MapsDependencyFailureToStablePublicError(string arrival, string code, string message)
    {
        using var services = CreateServices();
        var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
            .FlightStatusByRoute("LHR", arrival, FlightDate, null, null, null, CancellationToken.None));

        var graphError = Assert.Single(error.Errors);
        Assert.Equal(code, graphError.Extensions!["code"]);
        Assert.Equal(message, graphError.Message);
        Assert.Null(graphError.Exception);
        Assert.Single(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    [Fact]
    public async Task RouteQuery_RejectsResponseAboveConfiguredSizeLimit()
    {
        const string setting = "FlightStatus:MaxResponseBytes";
        var previous = ConfigReader.Try(setting);
        try
        {
            ConfigReader.Set(setting, "1024");
            using var services = CreateServices();
            var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
                .FlightStatusByRoute("LHR", "SFO", FlightDate, null, null, null, CancellationToken.None));

            var graphError = Assert.Single(error.Errors);
            Assert.Equal("FLIGHT_STATUS_UNAVAILABLE", graphError.Extensions!["code"]);
            Assert.Null(graphError.Exception);
            Assert.Single(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
        }
        finally
        {
            ConfigReader.Set(setting, previous!);
        }
    }

    [Theory]
    [InlineData("1", "5")]
    [InlineData("5", "1")]
    public async Task RouteQuery_MapsConfiguredDeadlineToTimeout(string totalSeconds, string attemptSeconds)
    {
        const string totalSetting = "FlightStatus:GraphQLTimeoutSeconds";
        const string attemptSetting = "FlightStatus:GraphQLAttemptTimeoutSeconds";
        var previousTotal = ConfigReader.Try(totalSetting);
        var previousAttempt = ConfigReader.Try(attemptSetting);
        try
        {
            ConfigReader.Set(totalSetting, totalSeconds);
            ConfigReader.Set(attemptSetting, attemptSeconds);
            using var services = CreateServices();
            var error = await Assert.ThrowsAsync<GraphQLException>(() => services.GetRequiredService<FlightStatusQuery>()
                .FlightStatusByRoute("LHR", "SEA", FlightDate, null, null, null, CancellationToken.None));

            var graphError = Assert.Single(error.Errors);
            Assert.Equal("FLIGHT_STATUS_TIMEOUT", graphError.Extensions!["code"]);
            Assert.Null(graphError.Exception);
        }
        finally
        {
            ConfigReader.Set(totalSetting, previousTotal!);
            ConfigReader.Set(attemptSetting, previousAttempt!);
        }
    }

    [Fact]
    public async Task DelayedHttpMock_YieldsAndHonoursCallerCancellation()
    {
        using var services = CreateServices();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{BaseUrl}/flightstatus/routes/LHR/SEA/2026-10-14");
        request.Headers.Add("Ocp-Apim-Subscription-Key", "test-subscription-key");
        request.Headers.Add("Cache-Control", "no-cache");
        var response = services.GetRequiredService<IHttpConnector>().SendAsync(request, cancellation.Token);

        Assert.False(response.IsCompleted, "The delayed mock must yield instead of blocking the caller.");
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => response);
        Assert.Empty(services.GetRequiredService<HttpClientFactory>().GetResponses(BaseUrl));
    }

    private static ServiceProvider CreateServices()
    {
        ConfigReader.Set("FlightStatus:InternalApiUrl", BaseUrl);
        ConfigReader.Set("FlightStatus:SubscriptionKey", "test-subscription-key");
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<TimeProvider>(new FixedTimeProvider())
            .AddSingleton<HttpClientFactory>()
            .AddSingleton<IHttpClientFactory>(provider => provider.GetRequiredService<HttpClientFactory>())
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IFlightStatusProvider, FlightStatusProvider>()
            .AddSingleton<FlightStatusQuery>()
            .BuildServiceProvider();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
    }
}
