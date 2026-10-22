using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using AirportAndTerminal = VAA.EAPI.GraphQL.Domain.PaxOps.Models.AirportAndTerminal;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SavePassportMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ICheckInServiceProvider _checkInServiceProvider;
        private IReferenceServiceProvider _referenceServiceProvider;
        private readonly SavePassportMutation _mutation;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<AnalyticsService> _loggerAnalytics;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogAnalytics _logAnalytics;
        private readonly CheckInQuery _checkInQuery;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<TelemetryService> _loggerTelemetry;
        private readonly ITelemetryService _telemetryService;
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();


        public SavePassportMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerAnalytics = Microsoft.Extensions.Logging.Abstractions.NullLogger<AnalyticsService>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _loggerTelemetry = Microsoft.Extensions.Logging.Abstractions.NullLogger<TelemetryService>.Instance;
            _telemetry = new TelemetryClient(new TelemetryConfiguration());
            _checkInServiceProvider = new CheckInServiceProvider(_httpConnector);
            _referenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _analyticsService = new AnalyticsService(_loggerAnalytics, _httpConnector);
            _logAnalytics = new LogAnalytics(_analyticsService);
            _telemetryService = new TelemetryService(_telemetry, _loggerTelemetry);
            _mutation = new SavePassportMutation(_checkInServiceProvider, _referenceServiceProvider, _logAnalytics, _telemetryService);
            _checkInQuery = new CheckInQuery(_checkInServiceProvider, _logAnalytics, _telemetryService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(null, _referenceServiceProvider, _logAnalytics, _telemetryService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenReferenceServiceProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, null, _logAnalytics, _telemetryService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLogAnalyticsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, _referenceServiceProvider, null, _telemetryService));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, _referenceServiceProvider, _logAnalytics, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullRequest()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.SavePassport(null, null));
        }

        [Fact]
        public async Task Mutation_ShouldReturnCheckInResponse_ForGoodResponse()
        {
            var passengerId = "01.01|good|response|2387bb6a-b781-48a9-acf2-1def8b0475ea";
            var givenNames = "LAMONT";
            var surname = "PAIVA";
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, "travelpartyid", true, "TEST02", "LHR", [passengerId], givenNames, surname);
            
            //Act
            var result = await _mutation.SavePassport(new Models.SavePassport.SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = passengerId,
                IsScanned = false,
                Document = new Models.Document
                {
                    DocumentType = Models.Enums.DocumentType.PASSPORT,
                    GivenNames = givenNames,
                    Surname = surname,
                    DateOfBirth = "1985-10-19",
                    Gender = new Gender
                    {
                        Code = "M"
                    },
                    DocumentNumber = "986708765",
                    ExpirationDate = "2033-10-19",
                    IssuingCountry = new IssuingCountry { Code = "USA" },
                    Nationality = new Nationality { Code = "USA" },
                    CountryOfResidence = new CountryOfResidence { Code = "USA" }
                }
            }, null);

            //Assert
            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.IsType<Models.OnlineCheckInStatus>(result.OnlineCheckInStatus);
            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02", await CheckInIdHelper.GetPnrLocator(result.CheckInId));
            Assert.NotEmpty(result.Passengers.FirstOrDefault().Documents);
            Assert.Equal("986708765", result.Passengers.FirstOrDefault().Documents.FirstOrDefault().DocumentNumber);
            Assert.Equal("GBR", result.Passengers.FirstOrDefault().Documents.FirstOrDefault().Nationality.Code);
            Assert.Equal("GBR", result.Passengers.FirstOrDefault().Documents.FirstOrDefault().IssuingCountry.Code);
            Assert.Equal("GBR", result.Passengers.FirstOrDefault().Documents.FirstOrDefault().CountryOfResidence.Code);
            Assert.Equal(Models.Enums.DocumentType.PASSPORT, result.Passengers.FirstOrDefault().Documents.FirstOrDefault().DocumentType);
            Assert.Equal(ContactTracingStatus.TO_COMPLETE, result.Passengers.FirstOrDefault().Requirements.ContactTracing);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.PassportProvided);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.PassportVerified);
            Assert.Equal(ReturnDateStatus.REQUIRED, result.Passengers.FirstOrDefault().Requirements.ReturnDateStatus);
            Assert.Equal(SecondaryDocumentStatus.REQUIRED, result.Passengers.FirstOrDefault().Requirements.SecondaryDocumentStatus);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.CheckinEligible);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.CheckedIn);
        }

        [Fact]
        public async Task Mutation_ShouldReturnError_WhenPassengerHasError()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, "travelpartyid", true, "TEST02", "LHR", ["01.01|name|mismatch|error"], "LAMONT", "PAIVA");

            //Act
            var result = await _mutation.SavePassport(new Models.SavePassport.SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = "01.01|name|mismatch|error",
                Document = new Models.Document
                {
                    BirthDate = new LocalDate(1990, 1, 1),
                    ExpiryDate = new LocalDate(2035, 12, 31),
                    GivenNames = "Test$"
                }
            }, new Mock<IResolverContext>().Object);

            //Assert
            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.IsType<Models.OnlineCheckInStatus>(result.OnlineCheckInStatus);
            Assert.Equal("travelrequirement.030.001", result.OnlineCheckInStatus.Code);
            Assert.Equal("The surname and given name entered does not match the reservation.", result.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task Mutation_ShouldReturnCountryName_IfReturnDateisRequired()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, "travelpartyid", true, "TEST02", "LHR", ["01.01|return|date|required"], "LAMONT", "PAIVA");

            //Act
            var result = await _mutation.SavePassport(new Models.SavePassport.SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = "01.01|return|date|required",
                Document = new Models.Document
                {
                    DateOfBirth = "1990-01-01",
                    ExpirationDate = "2024-01-01",
                    DocumentNumber = "986708888"
                }
            }, new Mock<IResolverContext>().Object);

            //Assert
            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.Single(result.Passengers);
            Assert.Equal("TEST02", result.Reference.PnrLocator);
            Assert.IsType<AirportAndTerminal>(result.CurrentSlice.Destination);
            Assert.Equal("USA", result.CurrentSlice.Destination.CountryCode);
            Assert.Equal("United States Of America", result.CurrentSlice.Destination.CountryName);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
        }

        [Fact]
        public async Task Mutation_ShouldNotReturnError_ForVisaRequirementWhenReturnDateisNeeded()
        {
            var passengerId = "01.01|return|date|required";
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, "travelpartyid", true, "TEST02", "LHR", [passengerId], "ANNA", "GELZINIS");

            //Act
            var result = await _mutation.SavePassport(new Models.SavePassport.SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = passengerId,
                Document = new Document
                {
                    DateOfBirth = "1984-05-13",
                    ExpirationDate = "2032-02-12"
                }
            }, new Mock<IResolverContext>().Object);

            //Assert
            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.NotNull(result.CheckInId);
            Assert.Equal(ReturnDateStatus.REQUIRED, result.Passengers.First(x => x.PassengerId == passengerId).Requirements.ReturnDateStatus);
            Assert.Null(result.Passengers.First(x => x.PassengerId == passengerId).Requirements.ReturnDate);
            Assert.Equal(SecondaryDocumentStatus.REQUIRED, result.Passengers.First(x => x.PassengerId == passengerId).Requirements.SecondaryDocumentStatus);
        }

        [Fact]
        public async Task Mutation_ShouldNotReturnError_ForGermanPassportCountryCode()
        {
            var passengerId = "01.01|german|passport|3487bb6a-b781-48a9-acf2-1def8b0475gA";
            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, "travelpartyid", true, "TEST02", "LHR", [passengerId], "TESTING", "ESTAB");
            
            //Act
            var result = await _mutation.SavePassport(new Models.SavePassport.SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = passengerId,
                Document = new Document
                {
                    BirthDate = new LocalDate(1984, 5, 13),
                    ExpiryDate = new LocalDate(2030, 5, 21),
                    CountryOfResidence = new CountryOfResidence { Code = "D" },
                    Nationality = new Nationality { Code = "D" },
                    IssuingCountry = new IssuingCountry { Code = "D" }
                }
            }, new Mock<IResolverContext>().Object);

            //Assert
            Assert.NotNull(result.CheckInId);
            Assert.NotNull(result.Passengers.First().Documents);
            Assert.Equal("D", result.Passengers.First().Documents.First().IssuingCountry.Code);
            Assert.Equal("D", result.Passengers.First().Documents.First().Nationality.Code);
            Assert.Equal("D", result.Passengers.First().Documents.First().CountryOfResidence.Code);
        }
    }
}
