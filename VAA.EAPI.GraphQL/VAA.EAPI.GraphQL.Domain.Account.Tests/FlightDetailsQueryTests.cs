using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class FlightDetailsQueryTests
    {
        private readonly Mock<ILogger<FlightDetailsQuery>> _mockLogger;
        private readonly Mock<ITokenB2CProvider<string>> _tokenb2cProvider;
        private readonly FlightDetailsQuery _query;
        private readonly Mock<IFlightQueryProvider> _flightQueryProvider;

        public FlightDetailsQueryTests()
        {
            _tokenb2cProvider = new Mock<ITokenB2CProvider<string>>();
            _mockLogger = new Mock<ILogger<FlightDetailsQuery>>();
            _flightQueryProvider = new Mock<IFlightQueryProvider>();
            _query = new FlightDetailsQuery(_mockLogger.Object, _tokenb2cProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightDetailsQuery(null, _tokenb2cProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightDetailsQuery(_mockLogger.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokenProviderObjectandLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FlightDetailsQuery(null, null));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullLoginToken()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.GetFlightDetails(_flightQueryProvider.Object, 1, 10));
        }

        [Fact]
        public async Task Query_ShouldThorwException_AfterFailingtoConnecttoIdentityMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authToken = TestDataProvider.GetTestData<string>("Account//AuthToken");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//AuthTokenFailedResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            _tokenb2cProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetFlightDetails(_flightQueryProvider.Object, 1, 10));
        }

        [Fact]
        public async Task Query_ShouldReturnPersonalInfo_ForGoodRequest()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authToken = TestDataProvider.GetTestData<string>("Account//AuthToken");
            var bookingResponse = TestDataProvider.GetTestData<BookingDetailsResponse.BookingResponse>("Account//BookingDetailsResponse");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            APIResponse<EAPIResponse> response = new()
            {
                Response = new()
                {
                    Value = "9999998173"
                }
            };
            APIResponse<BookingDetailsResponse.BookingResponse> aPIResponse = new()
            {
                Response = bookingResponse
            };

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken },
                { "Page", 1 },
                { "PageSize", 10 }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            FlightDetailsResponse flightDetailsResponses = new()
            {
                FlightSegmentDetails = new List<FlightSegmentDetails>()
                {
                    new FlightSegmentDetails()
                    {
                        OperatingAirlineName="VS",
                        OperatingAirlineCode="Virgin Atlantic Airways",
                        DateOfFlight= (LocalDate)Common.Helper.DateTimeHelper.ParseVaaBookingDate("20MAR30"),
                        ScheduledDepartureTime=(LocalTime)Common.Helper.DateTimeHelper.ParseVaaBookingTime("1050A") ,
                        OriginAirportName= "Indira Gandhi International Airport",
                        OriginAirportCountry= "India",
                        OriginAirportCode=  "DEL",
                        DestinationAirportName= "Heathrow Airport",
                        DestinationAirportCountry= "United Kingdom",
                        DestinationAirportCode= "LHR",
                        FlightNumber= "0302",
                        NumberOfDaysToDeparture= 13,
                        BookingReference= "DOHBBR"
                    },
                    new FlightSegmentDetails()
                    {
                        OperatingAirlineName="VS",
                        OperatingAirlineCode="Virgin Atlantic Airways",
                        DateOfFlight=(LocalDate)Common.Helper.DateTimeHelper.ParseVaaBookingDate("20APR30"),
                        ScheduledDepartureTime=(LocalTime)Common.Helper.DateTimeHelper.ParseVaaBookingTime("1250P") ,
                        OriginAirportName= "Heathrow Airport",
                        OriginAirportCountry= "United Kingdom",
                        OriginAirportCode=  "LHR",
                        DestinationAirportName= "Indira Gandhi International Airport",
                        DestinationAirportCountry= "India",
                        DestinationAirportCode= "DEL",
                        FlightNumber= "0303",
                        NumberOfDaysToDeparture= 165,
                        BookingReference= "DOHBBR"
                    }
                }
            };

            _tokenb2cProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);

            _flightQueryProvider.Setup(x => x.GetFlightsQueryData(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(flightDetailsResponses);
            var result = await _query.GetFlightDetails(_flightQueryProvider.Object, 1, 10);

            //Assert
            Assert.NotNull(result.FlightSegmentDetails.FirstOrDefault());
            Assert.Equal("DOHBBR", result.FlightSegmentDetails.First().BookingReference);
        }
    }
}
