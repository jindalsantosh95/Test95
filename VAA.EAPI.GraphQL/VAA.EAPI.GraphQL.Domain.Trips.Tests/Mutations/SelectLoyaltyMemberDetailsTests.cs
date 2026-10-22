using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Domain.Trips.Services.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SelectLoyaltyMemberDetailsTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILoyaltyMemberService _loyaltyMemberService;
        private readonly ManangeLoyaltyMemberMutation _manangeLoyaltyMemberMutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly IPassengerDetailsProvider _passengerDetailsProvider;
        private readonly ILoyaltyActionHandler _loyaltyActionHandler;
        private readonly ILoyaltyActionStrategy _loyaltyActionStrategy;
        private readonly ILoyaltyRequestValidator _requestValidator;
        private readonly ILogger<LoyaltyMemberService> _loggerLoyaltyService;
        private readonly ILogger<ManangeLoyaltyMemberMutation> _loggerLoyaltyMemberMutation;
        private readonly ILogger<LoyaltyActionHandler> _loggerLoyaltyActionHanlder;
        private readonly ILogger<LoyaltyActionStrategy> _loggerLoyaltyActionStrategy;

        public SelectLoyaltyMemberDetailsTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerLoyaltyActionHanlder = Microsoft.Extensions.Logging.Abstractions.NullLogger<LoyaltyActionHandler>.Instance;
            _loggerLoyaltyMemberMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<ManangeLoyaltyMemberMutation>.Instance;
            _loggerLoyaltyActionStrategy = Microsoft.Extensions.Logging.Abstractions.NullLogger<LoyaltyActionStrategy>.Instance;
            _loggerLoyaltyService = Microsoft.Extensions.Logging.Abstractions.NullLogger<LoyaltyMemberService>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _passengerDetailsProvider = new PassengerDetailsProvider(_httpConnector);
            _requestValidator = new LoyaltyRequestValidator();
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _loyaltyActionStrategy = new LoyaltyActionStrategy(_loggerLoyaltyActionStrategy);
            _loyaltyActionHandler = new LoyaltyActionHandler(_passengerDetailsProvider, _loggerLoyaltyActionHanlder);
            _loyaltyMemberService = new LoyaltyMemberService(_passengerDetailsProvider, _mockReferenceServiceProvider, _loyaltyActionHandler,
                _loyaltyActionStrategy, _requestValidator, _loggerLoyaltyService);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _loggerLoyaltyMemberMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<ManangeLoyaltyMemberMutation>.Instance;
            _manangeLoyaltyMemberMutation = new ManangeLoyaltyMemberMutation(_loyaltyMemberService, _loggerLoyaltyMemberMutation);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task AddLoyaltyMmebrDetailsForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";
            var request = new LoyaltyMemberDetailsRequest()
            {
                LoyaltyMemberId = "1234567890",
                LoyaltyProgramCode = "VS",
                PassengerId = passengerId,
                TripReservationId = tripReservationId

            };

            var vsProgramDetails = new Models.ReferenceData.FQTProgramRefData() { AirlineName = "Virgin Atlantic", MembNumRegex = "", ProgramCode = "VS", ProgramName = "Flying Club" };

            // Act
            var result = await _loyaltyActionHandler.ExecuteLoyaltyActionAsync(request, actionType: LoyaltyActionType.Add, vsProgramDetails, null, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateLoyaltyMmebrDetailsForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac6", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";
            var request = new LoyaltyMemberDetailsRequest()
            {
                LoyaltyMemberId = "3344456678",
                LoyaltyProgramCode = "DL",
                PassengerId = passengerId,
                TripReservationId = tripReservationId

            };


            IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts = [
               new LoyaltyProgramAccount
            {
                Number = "3344456678",
                LoyaltyProgram = new()
                {
                    Name = "SkyMiles",
                    Owner = [ new Carrier { Code = "DL", Name = "Delta Air Lines" }
                    ]
                }
            } ];

            var programDetails = new Models.ReferenceData.FQTProgramRefData() { AirlineName = "Delta Air Lines", MembNumRegex = "", ProgramCode = "DL", ProgramName = "SkyMiles" };

            // Act
            var result = await _loyaltyActionHandler.ExecuteLoyaltyActionAsync(request, actionType: LoyaltyActionType.Update, programDetails, existingLoyaltyProgramAccounts, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteLoyaltyMmebrDetailsForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac6", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";
            var request = new LoyaltyMemberDetailsRequest()
            {
                LoyaltyMemberId = "",
                LoyaltyProgramCode = "DL",
                PassengerId = passengerId,
                TripReservationId = tripReservationId

            };


            IEnumerable<LoyaltyProgramAccount> existingLoyaltyProgramAccounts = [
               new LoyaltyProgramAccount
            {
                Number = "3344456678",
                LoyaltyProgram = new()
                {
                    Name = "SkyMiles",
                    Owner = [ new Carrier { Code = "DL", Name = "Delta Air Lines" }
                    ]
                }
            } ];

            var programDetails = new Models.ReferenceData.FQTProgramRefData() { AirlineName = "Delta Air Lines", MembNumRegex = "", ProgramCode = "DL", ProgramName = "SkyMiles" };

            // Act
            var result = await _loyaltyActionHandler.ExecuteLoyaltyActionAsync(request, actionType: LoyaltyActionType.Delete, programDetails, existingLoyaltyProgramAccounts, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddDuplicateLoyaltyMmebrDetailsForTripMutation_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";
            var request = new LoyaltyMemberDetailsRequest()
            {
                LoyaltyMemberId = "1234567890",
                LoyaltyProgramCode = "VS",
                PassengerId = passengerId,
                TripReservationId = tripReservationId
            };

            var vsProgramDetails = new Models.ReferenceData.FQTProgramRefData() { AirlineName = "Virgin Atlantic", MembNumRegex = "", ProgramCode = "VS", ProgramName = "Flying Club" };

            // Act
            try
            {
                var result = await _manangeLoyaltyMemberMutation.SelectLoyaltyMemberDetails(request, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Equal(Constants.ErrorCodes.ManageLoyaltyProgrammeErrorCode, ex.InnerException.Message);
            }           
        }

        [Fact]
        public async Task UpdateLoyaltyMemberDetailsForTripMutation_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";
            var request = new LoyaltyMemberDetailsRequest()
            {
                LoyaltyMemberId = "3334445678",
                LoyaltyProgramCode = "DL",
                PassengerId = passengerId,
                TripReservationId = tripReservationId

            };

            // Act
            var result = await _manangeLoyaltyMemberMutation.SelectLoyaltyMemberDetails(request, CancellationToken.None);

            // Assert
            Assert.Equal("Success", result.Message);
        }
    }
}
