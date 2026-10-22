using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Http.Mocking;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries;

public sealed class TripDetailsListQueryResponseMockTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestExecutor _executor;
    private readonly string _projectRoot;

    public TripDetailsListQueryResponseMockTests()
    {
        _projectRoot = FindProjectRoot();

        Setup.CreateTestEnvironmentSettings();
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };

        ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:Url", "https://testresources.virginatlantic.com/dev/digital/holidays");
        ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:OcpSubID", Guid.NewGuid().ToString());

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>();

        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(new DateTime(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc)));

        new VAA.EAPI.GraphQL.Domain.Trips.Startup().ConfigureServices(services);
        services.AddGraphQLCore();

        _serviceProvider = services.BuildServiceProvider();
        _executor = SchemaBuilder.New()
            .AddServices(_serviceProvider)
            .AddQueryType(descriptor => descriptor.Name("Query"))
            .AddType(typeof(TripDetailsListQuery))
            .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
            .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
            .AddType<HotChocolate.Types.NodaTime.LocalDateType>()
            .Create()
            .MakeExecutable();
    }

    [Fact]
    public async Task TripDetails_EligibleVsLeg_PopulatesAircraftTailNumberAndStarlinkEligibility()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery("""
                query TripDetailsStarlink {
                  TripDetails(request: { tripDetails: [{ pnrLocator: "C4F2RI", surname: "Clarke", givenNames: "C" }] }) {
                    product {
                      slices {
                        segments {
                          legs {
                            aircraft { tailNumber }
                            isStarlinkEligible
                          }
                        }
                      }
                    }
                  }
                }
                """)
            .SetServices(_serviceProvider)
            .Create();

        var result = await _executor.ExecuteAsync(request);
        var queryResult = Assert.IsAssignableFrom<IQueryResult>(result);
        Assert.Empty(queryResult.Errors ?? []);

        var json = Newtonsoft.Json.Linq.JObject.Parse(
            Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, object?> { ["data"] = queryResult.Data }));
        var leg = json["data"]?["TripDetails"]?[0]?["product"]?[0]?["slices"]?[0]?["segments"]?[0]?["legs"]?[0];

        Assert.Equal("G-VTOM", leg?["aircraft"]?["tailNumber"]?.Value<string>());
        Assert.True(leg?["isStarlinkEligible"]?.Value<bool>());
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime Now => utcNow.ToLocalTime();
        public DateTime UtcNow => utcNow;
    }

    private static string FindProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(System.IO.Path.Combine(directory.FullName, "VAA.EAPI.GraphQL.Domain.Trips.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate Trips test project root.");
    }

    [Fact]
    public async Task TripDetails_SingleSliceHolidayFlights_MatchesExpectedGraphQLResponse()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery(
                """
                query TripDetailsSingleSliceHolidayFlights {
                  TripDetails(
                    request: {
                      tripDetails: [
                        {
                          bookingReference: "9999999"
                          surname: "User"
                          dateOfBirth: "1970-01-01"
                        }
                      ]
                    }
                  ) {
                    status
                    id
                    travelDates {
                      departureDate
                      returnDate
                    }
                    product {
                      bookingType
                      supportedFlight
                      sortByDate
                      reference {
                        pnrLocator
                        orderId
                      }
                      slices {
                        scheduledDepartureLocal
                        scheduledArrivalLocal
                        origin { code }
                        destination { code }
                      }
                    }
                  }
                }
                """)
            .SetServices(_serviceProvider)
            .Create();

        var result = await _executor.ExecuteAsync(request);

        var queryResult = Assert.IsAssignableFrom<IQueryResult>(result);
        if (queryResult.Errors is { Count: > 0 })
        {
            var firstError = queryResult.Errors[0];
            var exception = firstError.Exception;

            var httpRequestException = FindInnerException<HttpRequestException>(exception);

            if (httpRequestException is not null)
            {
                var requestUri = httpRequestException.Data["RequestUri"]?.ToString() ?? "<missing>";
                var requestBody = httpRequestException.Data["Request"]?.ToString() ?? "<missing>";
                var responseBody = httpRequestException.Data["Response"]?.ToString() ?? "<missing>";
                var requestVersion = (httpRequestException.InnerException as VAA.EAPI.GraphQL.Common.Exceptions.HttpResponseException)?.Response?.RequestMessage?.Version.ToString() ?? "<missing>";

                throw new Exception($"Downstream call was not mocked. RequestUri: {requestUri}{Environment.NewLine}HTTP Version: {requestVersion}{Environment.NewLine}Request: {requestBody}{Environment.NewLine}Response: {responseBody}");
            }

            throw new Exception($"GraphQL returned errors. First error: {firstError.Message}");
        }

        var actualJson = Newtonsoft.Json.JsonConvert.SerializeObject(
            new Dictionary<string, object?> { ["data"] = queryResult.Data },
            new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ss"
            });

        var actual = JToken.Parse(actualJson);

        var expectedPath = System.IO.Path.Combine(_projectRoot, "Queries", "ExpectedResponses", "tripDetails_singleSliceHolidayFlights.json");
        var expectedText = await File.ReadAllTextAsync(expectedPath);
        using (var expectedParser = (CommandParser)expectedText)
        {
            expectedText = expectedParser.Parse();
        }

        var expected = JToken.Parse(expectedText);

        if (!JToken.DeepEquals(expected, actual))
        {
            throw new Exception(
                $"GraphQL response JSON did not match expected fixture.{Environment.NewLine}" +
                $"Expected fixture: {expectedPath}{Environment.NewLine}" +
                $"Actual:{Environment.NewLine}{actual.ToString(Newtonsoft.Json.Formatting.Indented)}");
        }
    }


    [Fact]
    public async Task TripDetails_RetainsPastHolidayFlightSlices_WhenTripDetailsMissingPastSlices_MatchesExpectedGraphQLResponse()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery(
                """
                query TripDetailsRetainPastSlices {
                  TripDetails(
                    request: {
                      tripDetails: [
                        {
                          bookingReference: "8883814"
                          surname: "User"
                          dateOfBirth: "1970-01-01"
                        }
                      ]
                    }
                  ) {
                    status
                    id
                    travelDates {
                      departureDate
                      returnDate
                    }
                    product {
                      bookingType
                      supportedFlight
                      sortByDate
                      reference {
                        pnrLocator
                        orderId
                      }
                      slices {
                        scheduledDepartureLocal
                        scheduledArrivalLocal
                        origin { code }
                        destination { code }
                      }
                    }
                  }
                }
                """
            )
            .SetServices(_serviceProvider)
            .Create();

        var result = await _executor.ExecuteAsync(request);

        var queryResult = Assert.IsAssignableFrom<IQueryResult>(result);
        if (queryResult.Errors is { Count: > 0 })
        {
            var firstError = queryResult.Errors[0];
            var exception = firstError.Exception;

            var httpRequestException = FindInnerException<HttpRequestException>(exception);

            if (httpRequestException is not null)
            {
                var requestUri = httpRequestException.Data["RequestUri"]?.ToString() ?? "<missing>";
                var requestBody = httpRequestException.Data["Request"]?.ToString() ?? "<missing>";
                var responseBody = httpRequestException.Data["Response"]?.ToString() ?? "<missing>";
                var requestVersion = (httpRequestException.InnerException as VAA.EAPI.GraphQL.Common.Exceptions.HttpResponseException)?.Response?.RequestMessage?.Version.ToString() ?? "<missing>";

                throw new Exception($"Downstream call was not mocked. RequestUri: {requestUri}{Environment.NewLine}HTTP Version: {requestVersion}{Environment.NewLine}Request: {requestBody}{Environment.NewLine}Response: {responseBody}");
            }

            throw new Exception($"GraphQL returned errors. First error: {firstError.Message}");
        }

        var actualJson = Newtonsoft.Json.JsonConvert.SerializeObject(
            new Dictionary<string, object?> { ["data"] = queryResult.Data },
            new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ss"
            });

        var actual = JToken.Parse(actualJson);

        var expectedPath = System.IO.Path.Combine(_projectRoot, "Queries", "ExpectedResponses", "tripDetails_retainPastHolidayFlightSegments.json");
        var expectedText = await File.ReadAllTextAsync(expectedPath);
        using (var expectedParser = (CommandParser)expectedText)
        {
            expectedText = expectedParser.Parse();
        }

        var expected = JToken.Parse(expectedText);

        if (!JToken.DeepEquals(expected, actual))
        {
            throw new Exception(
                $"GraphQL response JSON did not match expected fixture.{Environment.NewLine}" +
                $"Expected fixture: {expectedPath}{Environment.NewLine}" +
                $"Actual:{Environment.NewLine}{actual.ToString(Newtonsoft.Json.Formatting.Indented)}");
        }
    }

    
    [Fact]
    public async Task TripDetails_RetainsPastHolidayFlightSlices_WhenTripDetailsOnlyReturnsFutureSliceWithTimeShift_MatchesExpectedGraphQLResponse()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery(
                """
                query TripDetailsRetainPastSlicesWithTimeShift {
                  TripDetails(
                    request: {
                      tripDetails: [
                        {
                          bookingReference: "7777777"
                          surname: "User"
                          dateOfBirth: "1970-01-01"
                        }
                      ]
                    }
                  ) {
                    status
                    id
                    travelDates {
                      departureDate
                      returnDate
                    }
                    product {
                      bookingType
                      supportedFlight
                      sortByDate
                      reference {
                        pnrLocator
                        orderId
                      }
                      slices {
                        scheduledDepartureLocal
                        scheduledArrivalLocal
                        origin { code }
                        destination { code }
                      }
                    }
                  }
                }
                """
            )
            .SetServices(_serviceProvider)
            .Create();

        var result = await _executor.ExecuteAsync(request);

        var queryResult = Assert.IsAssignableFrom<IQueryResult>(result);
        if (queryResult.Errors is { Count: > 0 })
        {
            var firstError = queryResult.Errors[0];
            var exception = firstError.Exception;

            var httpRequestException = FindInnerException<HttpRequestException>(exception);

            if (httpRequestException is not null)
            {
                var requestUri = httpRequestException.Data["RequestUri"]?.ToString() ?? "<missing>";
                var requestBody = httpRequestException.Data["Request"]?.ToString() ?? "<missing>";
                var responseBody = httpRequestException.Data["Response"]?.ToString() ?? "<missing>";
                var requestVersion = (httpRequestException.InnerException as VAA.EAPI.GraphQL.Common.Exceptions.HttpResponseException)?.Response?.RequestMessage?.Version.ToString() ?? "<missing>";

                throw new Exception($"Downstream call was not mocked. RequestUri: {requestUri}{Environment.NewLine}HTTP Version: {requestVersion}{Environment.NewLine}Request: {requestBody}{Environment.NewLine}Response: {responseBody}");
            }

            throw new Exception($"GraphQL returned errors. First error: {firstError.Message}");
        }

        var actualJson = Newtonsoft.Json.JsonConvert.SerializeObject(
            new Dictionary<string, object?> { ["data"] = queryResult.Data },
            new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ss"
            });

        var actual = JToken.Parse(actualJson);

        var expectedPath = System.IO.Path.Combine(_projectRoot, "Queries", "ExpectedResponses", "tripDetails_retainPastHolidayFlights_timeShift.json");
        var expectedText = await File.ReadAllTextAsync(expectedPath);
        using (var expectedParser = (CommandParser)expectedText)
        {
            expectedText = expectedParser.Parse();
        }

        var expected = JToken.Parse(expectedText);

        if (!JToken.DeepEquals(expected, actual))
        {
            throw new Exception(
                $"GraphQL response JSON did not match expected fixture.{Environment.NewLine}" +
                $"Expected fixture: {expectedPath}{Environment.NewLine}" +
                $"Actual:{Environment.NewLine}{actual.ToString(Newtonsoft.Json.Formatting.Indented)}");
        }
    }

    private static TException? FindInnerException<TException>(Exception? exception)
        where TException : Exception
    {
        while (exception is not null)
        {
            if (exception is TException match)
                return match;

            exception = exception.InnerException;
        }

        return null;
    }
}
