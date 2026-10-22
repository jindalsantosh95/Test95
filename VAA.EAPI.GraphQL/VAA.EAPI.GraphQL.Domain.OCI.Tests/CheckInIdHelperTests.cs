using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class CheckInIdHelperTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly ICheckInServiceProvider _checkInServiceProvider;

        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public CheckInIdHelperTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _checkInServiceProvider = new CheckInServiceProvider(_httpConnector);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task CheckInIdHelper_ShouldReturnNullIDs_ForExpiredIdinToken()
        {
            ConfigReader.Set("CheckIn:RequirementIdExpiryTimeinMin", "-5");
            ConfigReader.Set("CheckIn:TravelPartyIdExpiryTimeinMin", "-5");

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId", true, "FAKPNR", "LHR", null, null, null);

            Assert.NotNull(checkInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(checkInId), "checkInId should be valid JWT");
            Assert.False(await CheckInIdHelper.CheckIdExpiryTime(checkInId, "requirementsId"), "requirementsId expiry time should be false");
            Assert.False(await CheckInIdHelper.CheckIdExpiryTime(checkInId, "travelPartyId"), "travelPartyId expiry time should be false");
            Assert.Null(await CheckInIdHelper.GetRequirementsId(checkInId));
            Assert.Null(await CheckInIdHelper.GetTravelPartyId(checkInId));
        }

        [Fact]
        public async Task CheckInIdHelper_ShouldReturnTravelPartyId_ForValidToken()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId", true, "FAKPNR", "LHR", null, null, null);

            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("travelPartyIdExpiryTime"));
            Assert.True(await CheckInIdHelper.CheckIdExpiryTime(checkInId, "travelPartyId"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("requirementsIdExpiryTime"));
            Assert.True(await CheckInIdHelper.CheckIdExpiryTime(checkInId, "requirementsId"));
            Assert.Equal("FakeTravelPartyId", await CheckInIdHelper.GetTravelPartyId(checkInId));
            Assert.Equal("FakeRequirementsId", await CheckInIdHelper.GetRequirementsId(checkInId));
        }

        [Fact]
        public async Task CheckInIdHelper_ShouldReturnPassengerIDs_ForValidToken()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId", true, "FAKPNR", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea", "02.01|49f4da79-ewge-4c46-a3a8-833e140cfde9|99992254679295|2387bb6a-ewrw-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName");

            Assert.True(jwtSecurityTokenHandler.CanReadToken(checkInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("selectedPassengers"));
            var selectedPassengers = await CheckInIdHelper.GetSelectedPassengers(checkInId);
            Assert.NotNull(selectedPassengers);
            Assert.Equal(2, selectedPassengers.Count());
            Assert.Equal("01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea", selectedPassengers.FirstOrDefault());
        }

        [Fact]
        public async Task CheckInIdHelper_ShouldReturnPNRandOriginCode_ForValidToken()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId", true, "FAKPNR", "LHR", null, "FakeFirstName", "FakeLastName");

            Assert.NotNull(checkInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(checkInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("pnrLocator"));
            Assert.Equal("FAKPNR", await CheckInIdHelper.GetPnrLocator(checkInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("originCode"));
            Assert.Equal("LHR", await CheckInIdHelper.GetOriginCode(checkInId));
        }

        [Fact]
        public async Task CheckInIdHelper_ShouldReturnFirstNamenandLastName_ForValidToken()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId", true, "FAKPNR", "LHR", null, "FakeFirstName", "FakeLastName");

            Assert.NotNull(checkInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(checkInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("firstName"));
            Assert.Equal("FakeFirstName", await CheckInIdHelper.GetFirstName(checkInId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(checkInId).Payload.ContainsKey("lastName"));
            Assert.Equal("FakeLastName", await CheckInIdHelper.GetLastName(checkInId));
        }
    }
}
