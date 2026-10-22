using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Claims;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Tests
{
    public sealed class FlightOperationsServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IAzureADServiceWrapper _azureADServiceWrapper;
        private readonly Dictionary<string, object> items = new();
        private readonly ILogger<FlightOperationsService> _flightOperationsLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightOperationsService>.Instance;

        public FlightOperationsServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _azureADServiceWrapper = new AzureADServiceWrapperStub();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext(items) };         
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(), ""));

            ConfigReader.Set("FlightOperation:OperationalScheduleAPIClientId", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("FlightOperation:APIM:OcpSubscriptionKey", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("FlightOperation:OperationalScheduleAPIClientSecret", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("FlightOperation:OperationalScheduleAPIResource", "00000000-0000-0000-0000-000000000000");
            ConfigReader.Set("FlightOperation:OperationalScheduleAPIAuthority", "https://login.microsoftonline.com/6bdfd5a8-531f-4f14-9af1-dc7b444c3ad8/"); 
        }

        [Fact]
        public async Task GetOperationalScheduleAsync_FailsForNullRequest()
        {
            //Arrange           
            var operationService = new FlightOperationsService(_httpConnector, _azureADServiceWrapper, _flightOperationsLogger);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => operationService.GetOperationalScheduleAsync(null, CancellationToken.None));
        }

        [Fact]
        public async Task GetOperationalScheduleAsync_ReturnsAircraftData()
        {
            //Arrange
            ConfigReader.Set("environment", "NONPROD");
            ConfigReader.Set("FlightOperation:APIM:Url", "https://Testresources.virginatlantic.com/enterprise/operationallogistics/managetheoperation/operationalschedule/flights?Airline_Code={Airline_Code}&Local_Date_From={Local_Date_From}&Local_Date_To={Local_Date_To}&Direction=D&Origin_Airport_Code={Origin_Airport_Code}&limit=500&Flight_Number={Flight_Number}");

            var operationService = new FlightOperationsService(_httpConnector, _azureADServiceWrapper, _flightOperationsLogger);
            var operationScheduleRequest = new OperationalScheduleRequest
            {
                AirlineCode = "VS",
                LocalDateFrom = DateTime.Parse("2026-01-16T05:00"),
                LocalDateTo = DateTime.Parse("2026-01-16T06:30"),
                OriginAirportCode = "LHR",
                FlightNumber = 103
            };

            //Act
            var result = await operationService.GetOperationalScheduleAsync(operationScheduleRequest, CancellationToken.None);
            var response = result.Value;

            //Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Value.Flights);
            Assert.NotNull(response.Value.Flights.First().Aircraft);
            Assert.Equal("G-VJAM", response.Value.Flights.First().Aircraft.Registration);
            Assert.Equal("Queen of Hearts", response.Value.Flights.First().Aircraft.Name);
            Assert.Equal("16-01-2026 10:45:00", response.Value.Flights.First().Legs.First().ScheduledDepartureTime.LocalDateTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            Assert.Equal("16-01-2026 10:45:00", response.Value.Flights.First().Legs.First().ScheduledDepartureTime.UTCDateTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            Assert.Equal("16-01-2026 15:15:00", response.Value.Flights.First().Legs.First().ScheduledArrivalTime.LocalDateTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            Assert.Equal("16-01-2026 20:15:00", response.Value.Flights.First().Legs.First().ScheduledArrivalTime.UTCDateTime.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private sealed class AzureADServiceWrapperStub : IAzureADServiceWrapper
        {
            public Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource)
            {
                return Task.FromResult("test-access-token");
            }

            public Task<string> GetAccessTokenV2(string clientId, string clientSecret, string authority, string scope)
            {
                return Task.FromResult("test-access-token");
            }
        }
    }
}
