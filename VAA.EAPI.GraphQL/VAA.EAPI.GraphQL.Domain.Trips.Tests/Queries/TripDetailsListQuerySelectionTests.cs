using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries;

[Collection("Flight Ops configuration")]
public sealed class TripDetailsListQuerySelectionTests
{
    private const string CheckInMsUrl = "https://Testresources.virginatlantic.com/dev/checkinmicroservice";
    private const string TravelRequirementsUrl = $"{CheckInMsUrl}/TravelRequirements";
    private const string FlightOpsUrl = "http://tempURL.org";
    private const string OperationalScheduleUrl = $"{FlightOpsUrl}/operationalschedule";
    private const string StarlinkEligibilityUrl = $"{FlightOpsUrl}/checkaircraftcategory";

    private readonly IServiceProvider _serviceProvider;
    private readonly XunitHttpClientFactory _httpClientFactory;
    private readonly IRequestExecutor _executor;
    private readonly FixedDateTimeProvider _dateTimeProvider;

    public TripDetailsListQuerySelectionTests()
    {
        Setup.CreateTestEnvironmentSettings();
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>();

        _dateTimeProvider = new FixedDateTimeProvider(new DateTime(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc));
        services.AddSingleton<IDateTimeProvider>(_dateTimeProvider);

        new VAA.EAPI.GraphQL.Domain.Trips.Startup().ConfigureServices(services);
        services.AddGraphQLCore();

        _serviceProvider = services.BuildServiceProvider();
        _httpClientFactory = (XunitHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>();
        _executor = SchemaBuilder.New()
            .AddServices(_serviceProvider)
            .AddQueryType(descriptor => descriptor.Name("Query"))
            .AddType(typeof(TripsQuery))
            .AddType(typeof(TripDetailsListQuery))
            .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
            .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
            .AddType<HotChocolate.Types.NodaTime.LocalDateType>()
            .Create()
            .MakeExecutable();
    }

    [Theory]
    [InlineData("areTravelDocsComplete")]
    [InlineData("travelDocsStatusSummary")]
    public async Task Booking_TravelDocumentFieldIsSelected_CallsTravelRequirementsProvider(string fieldName)
    {
        var responseCount = CountTravelRequirementsResponses();

        await ExecuteBookingQuery(fieldName);

        Assert.Equal(responseCount + 1, CountTravelRequirementsResponses());
    }

    [Fact]
    public async Task Booking_TravelDocumentFieldsAreNotSelected_DoesNotCallTravelRequirementsProvider()
    {
        var responseCount = CountTravelRequirementsResponses();

        await ExecuteBookingQuery("id");

        Assert.Equal(responseCount, CountTravelRequirementsResponses());
    }

    [Fact]
    public async Task TripDetails_TravelDocumentFieldsAreSelected_DoesNotCallTravelRequirementsProvider()
    {
        var responseCount = CountTravelRequirementsResponses();

        await ExecuteTripDetailsQuery("areTravelDocsComplete");

        Assert.Equal(responseCount, CountTravelRequirementsResponses());
    }

    [Fact]
    public async Task TripDetails_AircraftFieldsAreNotSelected_DoesNotCallAircraftServices()
    {
        var operationalScheduleCount = CountFlightOpsResponses(OperationalScheduleUrl);
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        await ExecuteAircraftSelectionQuery("status");

        Assert.Equal(operationalScheduleCount, CountFlightOpsResponses(OperationalScheduleUrl));
        Assert.Equal(starlinkCount, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_TailNumberSelected_CallsOperationalScheduleOnly()
    {
        var operationalScheduleCount = CountFlightOpsResponses(OperationalScheduleUrl);
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        await ExecuteAircraftSelectionQuery("aircraft { tailNumber }");

        Assert.Equal(operationalScheduleCount + 1, CountFlightOpsResponses(OperationalScheduleUrl));
        Assert.Equal(starlinkCount, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_StarlinkEligibilitySelected_CallsOperationalScheduleAndStarlink()
    {
        var operationalScheduleCount = CountFlightOpsResponses(OperationalScheduleUrl);
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        await ExecuteAircraftSelectionQuery("isStarlinkEligible");

        Assert.Equal(operationalScheduleCount + 1, CountFlightOpsResponses(OperationalScheduleUrl));
        Assert.Equal(starlinkCount + 1, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_BothAircraftFieldsSelected_CallsEachAircraftServiceOnce()
    {
        var operationalScheduleCount = CountFlightOpsResponses(OperationalScheduleUrl);
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        await ExecuteAircraftSelectionQuery("aircraft { tailNumber } isStarlinkEligible");

        Assert.Equal(operationalScheduleCount + 1, CountFlightOpsResponses(OperationalScheduleUrl));
        Assert.Equal(starlinkCount + 1, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_StarlinkServiceReturnsFalse_ReturnsFalseEligibility()
    {
        ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-false");

        var result = await ExecuteAircraftSelectionQuery("isStarlinkEligible");
        var leg = GetFirstLeg(result);

        Assert.False(leg?["isStarlinkEligible"]?.Value<bool>());
    }

    [Fact]
    public async Task TripDetails_AircraftNumberMissing_DoesNotCallAircraftServices()
    {
        _dateTimeProvider.SetUtcNow(new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc));
        var operationalScheduleCount = CountFlightOpsResponses(OperationalScheduleUrl);
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        await ExecuteAircraftSelectionQuery("aircraft { tailNumber } isStarlinkEligible", "AHDHDJ", "Winson", "S");

        Assert.Equal(operationalScheduleCount, CountFlightOpsResponses(OperationalScheduleUrl));
        Assert.Equal(starlinkCount, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_AircraftTailLookupFails_BookingSucceedsWithNullEligibility()
    {
        ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/error");
        var starlinkCount = CountFlightOpsResponses(StarlinkEligibilityUrl);

        var result = await ExecuteAircraftSelectionQuery("aircraft { tailNumber } isStarlinkEligible");
        var leg = GetFirstLeg(result);

        Assert.NotNull(result.Data);
        Assert.Null(leg?["aircraft"]?["tailNumber"]?.Value<string>());
        Assert.Null(leg?["isStarlinkEligible"]?.Value<bool?>());
        Assert.Equal(starlinkCount, CountFlightOpsResponses(StarlinkEligibilityUrl));
    }

    [Fact]
    public async Task TripDetails_SupportedHolidayPnr_StarlinkFieldSelected_EnrichesFlightProduct()
    {
        var result = await ExecuteHolidayAircraftSelectionQuery();
        var json = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(
            new Dictionary<string, object?> { ["data"] = result.Data }));
        var flightProduct = json["data"]?["TripDetails"]?[0]?["product"]?
            .FirstOrDefault(product => product?["supportedFlight"]?.Value<bool>() == true);
        var eligibility = flightProduct?["slices"]?[0]?["segments"]?[0]?["legs"]?[0]?["isStarlinkEligible"]?.Value<bool?>();

        Assert.True(eligibility);
    }

    [Fact]
    public async Task TripDetails_NewSegmentFieldsAreExposed()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery(
                """
                query TripDetailsNewSegmentFields {
                  TripDetails(
                    request: {
                      tripDetails: [
                        {
                          pnrLocator: "EK49YD"
                          givenNames: "BARBRA"
                          surname: "STARY"
                        }
                      ]
                    }
                  ) {
                    product {
                      slices {
                        segments {
                          popsOpenDateTimeUtc
                          popsCloseDateTimeUtc
                          otherAirlineOperatedHelpTitle
                          otherAirlineOperatedHelpText
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
    }

    private async Task ExecuteTripDetailsQuery(string sliceSelection)
    {
        var request = QueryRequestBuilder.New()
            .SetQuery($$"""
            query TripDetailsListQuery {
              TripDetails(
                request: {
                  tripDetails: [
                    {
                      pnrLocator: "EK49YD"
                      givenNames: "BARBRA"
                      surname: "STARY"
                    }
                  ]
                }
              ) {
                passengers {
                  slices {
                    {{sliceSelection}}
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
    }

    private async Task ExecuteBookingQuery(string sliceSelection)
    {
        var request = QueryRequestBuilder.New()
            .SetQuery($$"""
            query BookingQuery {
              Booking(
                tripDetailsRequest: {
                  using: PNR
                  confirmationNum: "ABCDEF"
                  givenNames: "givenName"
                  surname: "surName"
                }
              ) {
                passengers {
                  slices {
                    {{sliceSelection}}
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
    }

    private async Task<IQueryResult> ExecuteAircraftSelectionQuery(
        string legSelection,
        string pnrLocator = "C4F2RI",
        string givenNames = "C",
        string surname = "Clarke")
    {
        var request = QueryRequestBuilder.New()
            .SetQuery($$"""
            query TripDetailsListQuery {
              TripDetails(
                request: {
                  tripDetails: [
                    {
                      pnrLocator: "{{pnrLocator}}"
                      givenNames: "{{givenNames}}"
                      surname: "{{surname}}"
                    }
                  ]
                }
              ) {
                product {
                  slices {
                    segments {
                      legs {
                        {{legSelection}}
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
        return queryResult;
    }

    private static JObject? GetFirstLeg(IQueryResult result)
    {
        var json = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(
            new Dictionary<string, object?> { ["data"] = result.Data }));
        return json["data"]?["TripDetails"]?[0]?["product"]?[0]?["slices"]?[0]?["segments"]?[0]?["legs"]?[0] as JObject;
    }

    private async Task<IQueryResult> ExecuteHolidayAircraftSelectionQuery()
    {
        var request = QueryRequestBuilder.New()
            .SetQuery("""
            query HolidayTripDetails {
              TripDetails(
                request: {
                  tripDetails: [
                    {
                      bookingReference: "6996219"
                      dateOfBirth: "1988-11-29"
                      surname: "Walker"
                    }
                  ]
                }
              ) {
                product {
                  supportedFlight
                  slices {
                    segments {
                      legs {
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
        return queryResult;
    }

    private int CountTravelRequirementsResponses() =>
        _httpClientFactory.GetResponses(CheckInMsUrl).Count(response =>
            response.RequestMessage?.RequestUri?.ToString().Is(TravelRequirementsUrl) == true);

    private int CountFlightOpsResponses(string url) =>
        _httpClientFactory.GetResponses(FlightOpsUrl).Count(response =>
            response.RequestMessage?.RequestUri?.ToString().Is(url) == true);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        private DateTime _utcNow = utcNow;

        public DateTime Now => _utcNow.ToLocalTime();
        public DateTime UtcNow => _utcNow;

        public void SetUtcNow(DateTime value) => _utcNow = value;
    }
}
