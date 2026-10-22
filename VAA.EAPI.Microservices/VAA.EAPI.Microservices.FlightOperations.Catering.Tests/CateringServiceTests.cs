using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Mapper;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Service;
using VAA.EAPI.Microservices.FlightOperations.Catering.Services;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Tests
{
    public sealed class CateringServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CateringServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var test = GetBookingInfoRequestData();
            ConfigReader.Set("PopsSqlDbConnectionString", "testdb");
            ConfigReader.Set("PremiumEconomyConfigKey", "W,A,B,C");
            ConfigReader.Set("UpperClassConfigKey", "J,D,E,F");
            ConfigReader.Set("EconomyCompleteKey", "Y,G,H,I");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<ICateringService, CateringService>()
                                                  .AddSingleton<IPopsDbService, PopsDbService>()
                                                  .AddDbContext<PopsDataContext>(options =>
                                                   options.UseSqlServer(ConfigReader.Get(Constants.Environment.PopsSqlDbConnString)))
                                                  .BuildServiceProvider();            
        }


        [Fact]
        public void Handle_Validate_Cabin_Mapper_Success()
        {
            // Arrange & Act
            string cabin = CabinMapper.MapToCabin("A");
           
            //Assert           
            Assert.Equal("W", cabin);           
        }

        [Fact]
        public void Handle_Validate_Invalid_Cabin_Mapper_Success()
        {
            // Arrange & Act
            string cabin = CabinMapper.MapToCabin("z");

            //Assert           
            Assert.Equal("Z", cabin);

        }
        [Fact]
        public void Handle_Validate_Meal_Cabin_Non_Eligible_Segments()
        {
            // Arrange
            var request = GetBookingInfoRequestData();
            request.Passengers.First().FrequentFlyerNumber = string.Empty;
            request.Passengers.Last().FrequentFlyerNumber = string.Empty;
            request.Segments.First().Cabin = "Y";
            var _cateringService = _serviceProvider.GetService<ICateringService>()!;

            //Act
            var result = _cateringService.ValidateSegmentsMealEligibility(request);

            //Assert           
            Assert.Equal(2, result.Value.Value.Segments?.Count());
            Assert.Equal(string.Empty, result.Value.Value.Passengers.First().FrequentFlyerNumber);
            Assert.Equal(string.Empty, result.Value.Value.Passengers.Last().FrequentFlyerNumber);
            Assert.Equal(EligibilityType.DutyFree, result.Value.Value.Segments?.First().Eligibility);
            Assert.Equal("Cabin not eligible to order meals; Eligible for DF only", result.Value.Value.Segments?.Last().PopsNonEligibleReason);

        }


        [Fact]
        public void Handle_Validate_Non_Eligible_Segments_With_Invalid_Data()
        {
            // Arrange
            var request = GetBookingInfoRequestData();
            request.Segments.First().Cabin = string.Empty;
            request.Segments.Last().Cabin = string.Empty;
            var _cateringService = _serviceProvider.GetService<ICateringService>()!;

            //Act
            var result = _cateringService.ValidateSegmentsMealEligibility(request);

            //Assert           
            Assert.Equal(2, result.Value.Value.Segments?.Count());
            Assert.Equal(EligibilityType.None, result.Value.Value.Segments?.First().Eligibility);
            Assert.Equal("Data not found", result.Value.Value.Segments?.Last().PopsNonEligibleReason);

        }

        private BookingInfo GetBookingInfoRequestData() => new BookingInfo
        {
            RecordLocator = "ABC123",
            BookingDate = DateTime.UtcNow,
            Passengers = new List<Passenger>
            {
                new Passenger
                {
                    PassengerID = "P001",
                    FirstName = "John",
                    LastName = "Doe",
                    FrequentFlyerNumber = "FF123456"
                },
                new Passenger
                {
                    PassengerID = "P002",
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
                    OperatingFlight = new Flight
                    {
                        AirlineCode = "AI",
                        Number = 101
                    },
                    MarketingFlight = new Flight
                    {
                        AirlineCode = "UA",
                        Number = 9001
                    },
                    Departure = new DateTime(2025, 12, 01, 9, 0, 0, DateTimeKind.Utc),
                    Origin = "DEL",
                    Destination = "LHR",
                    Cabin = "J",
                },
                new Segment
                {
                    SegmentID = "S002",
                    OperatingFlight = new Flight
                    {
                        AirlineCode = "AI",
                        Number = 102
                    },
                    MarketingFlight = new Flight
                    {
                        AirlineCode = "UA",
                        Number = 9002
                    },
                    Departure = new DateTime(2025, 12, 10, 18, 0, 0, DateTimeKind.Utc),
                    Origin = "LHR",
                    Destination = "DEL",
                    Cabin = "Y",
                }
            }
        };

    }
}
