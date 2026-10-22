using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;
using VAA.EAPI.Microservices.FlightOperations.Catering.Services;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Tests
{
    public sealed class PopsServiceTests : IClassFixture<HttpClientFactoryFixture>, IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly PopsDataContext _ctx;
        private readonly PopsDbService _service;

        public PopsServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {           
            // Create one in-memory SQLite connection per test class instance
            _conn = new SqliteConnection("Data Source=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<PopsDataContext>()
                .UseSqlite(_conn)                
                .Options;

            _ctx = new PopsDataContext(options);
            _ctx.Database.EnsureCreated();

            _service = new PopsDbService(_ctx, NullLogger<PopsDbService>.Instance);
        }
             
        [Fact]
        public void Returns_EnabledOffer_When_AllFiltersMatch()
        {
            // Arrange
            var (cabin, depLoc, depUtc, airline, number, origin, dest) = GetData();

            // Act 
            var result = _service.CheckPopsEligibility(cabin,depLoc,airline,number,origin,dest).Result;

            // Assert
            Assert.NotNull(result);           
            Assert.Equal(24, result.MinBookingHour);
            Assert.Equal(168, result.MaxBookingHour);
            Assert.Equal(depUtc, result.DepartureDateUTC);
        }

        [Fact]
        public void Returns_null_When_FilterNotMatch()
        {
            // Arrange
            var (cabin, depLoc, depUtc, airline, number, origin, dest) = GetData();

            // Act 
            var result = _service.CheckPopsEligibility("G", depLoc, airline, number, origin, dest).Result;

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _conn.Close();
            _conn.Dispose();
        }

        private (string cabin, DateTime depLoc, DateTime depUtc, string airline, string number, string origin, string dest) GetData()
        {
            var cabin = "W";
            var airline = "AI";
            var number = "123";
            var origin = "DEL";
            var dest = "BOM";
            var depLoc = new DateTime(2026, 02, 07, 10, 00, 00, DateTimeKind.Unspecified); // local
            var depUtc = new DateTime(2026, 02, 07, 04, 30, 00, DateTimeKind.Utc);

            // Seed minimal relational data
            var flight = new Flight
            {
                FlightId = 1,
                OperatingAirline = airline,
                OperatingFlightNum = "123",
                ScheDepTimestampLoc = depLoc,
                ScheArrTimestampUtc = depUtc,
                Origin = origin,
                Destination = dest,
                OperatingFlightCodeSuffix = "1",
                ScheArrTimestampLoc = depLoc,
                ScheDepTimestampUtc = depUtc,
            };
            
            var leg = new FlightLeg
            {
                FlightId = 1,
                ScheDepTimestampLoc = depLoc,
                ScheDepTimestampUtc = depUtc,
                Origin = origin,
                Destination = dest,
                ScheArrTimestampLoc = depLoc,
                ScheArrTimestampUtc = depUtc,
                Flight = flight,
            };

            var menu = new Menu
            {
                Id = 1,
                Name = "test"
            };

            var psOlder = new PreselectOffer
            {
                Menus_Id = 1,
                Display_Sequence = 1,
                Menu = menu,
                Meal_Code = "SPML",
                Flight_Codes_Suffix = "VS",
                Enabled = true,
                Cabin_Code = cabin,
                Flight_Codes_Airline = airline,
                Flight_Codes_Number = "123",
                Offer_Effective_From = depUtc.AddDays(-5),
                Offer_Effective_To = depUtc.AddDays(+5),
                Bkg_Win_LWatermark = 24,
                Bkg_Win_HWatermark = 168,
                Created = new DateTime(2026, 01, 31, 08, 00, 00, DateTimeKind.Utc)
            };

            _ctx.Add(flight);
            _ctx.Add(leg);
            _ctx.AddRange(psOlder);
            _ctx.SaveChanges();

            return (cabin, depLoc, depUtc, airline, number, origin, dest);
        }

    }
}
