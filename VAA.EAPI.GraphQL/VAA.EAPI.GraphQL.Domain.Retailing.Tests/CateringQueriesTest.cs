using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;


namespace VAA.EAPI.GraphQL.Domain.Retailing.Tests
{
    public class CateringQueriesTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CateringQueriesTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("CateringProvider:CateringMsUri", "https://tempuri.org/catering");
            ConfigReader.Set("cateringprovider:OcpKey", "12345678");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<ICateringProvider, CateringProvider>()
                                                      .AddSingleton<CateringQueries>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Execute_CateringQueries_IsEligible()
        {
            var cateringQueries = _serviceProvider.GetService<CateringQueries>()!;

            var result = await cateringQueries.IsEligible(GetBookingInfoRequestData());

            Assert.Equal(Models.EligibilityType.POPS, result.Segments.First().Eligibility);
        }

        [Fact]
        public async Task Execute_CateringQueries_RetrieveFlightMeals()
        {
            var cateringQueries = _serviceProvider.GetService<CateringQueries>()!;

            string flightDate = "2025-10-10";
            string carrier = "VS";
            int flightNumber = 123;
            string cabin = "Economy";
            string pnr = "PNR123";

            var result = await cateringQueries.RetrieveFlightMeals(flightDate, carrier, flightNumber, cabin);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Single(result, x => x.Code.Is("test") && x.Description.Is("test ssr"));
        }

        private PNR GetBookingInfoRequestData() => new PNR
        {
            RecordLocator = "ABC123",
            BookingDate = DateTime.UtcNow,
            Passengers = new List<Passenger>
            {
                new Passenger
                {
                    PassengerId = "P001",
                    FirstName = "John",
                    LastName = "Doe",
                    FrequentFlyerNumber = "FF123456"
                },
                new Passenger
                {
                    PassengerId = "P002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    FrequentFlyerNumber = "FF654321"
                }
            },
            Segments = new List<Segment>
            {
                new Segment
                {
                    SegmentID = "S001",
                    OperatingFlight = new FlightNumber
                    {
                        AirlineCode = "AI",
                        Number = 101
                    },
                    MarketingFlight = new FlightNumber
                    {
                        AirlineCode = "UA",
                        Number = 9001
                    },
                    Departure = new DateTime(2025, 12, 01, 9, 0, 0, DateTimeKind.Utc),
                    Origin = "DEL",
                    Destination = "LHR",
                    Cabin = "J",
                    Eligibility = EligibilityType.DutyFree
                },
                new Segment
                {
                    SegmentID = "S002",
                    OperatingFlight = new FlightNumber
                    {
                        AirlineCode = "AI",
                        Number = 102
                    },
                    MarketingFlight = new FlightNumber
                    {
                        AirlineCode = "UA",
                        Number = 9002
                    },
                    Departure = new DateTime(2025, 12, 10, 18, 0, 0, DateTimeKind.Utc),
                    Origin = "LHR",
                    Destination = "DEL",
                    Cabin = "Y",
                    Eligibility = EligibilityType.POPS
                }
            }
        };
    }
}