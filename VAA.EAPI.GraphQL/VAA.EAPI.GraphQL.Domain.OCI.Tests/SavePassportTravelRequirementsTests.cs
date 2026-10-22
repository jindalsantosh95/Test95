using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SavePassportTravelRequirementsTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IResolverContext> _contextMock = new();
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public SavePassportTravelRequirementsTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ITravelRequirementsServiceProvider, TravelRequirementsServiceProvider>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<ITelemetryService, TelemetryService>()
                .AddLogging()
                .AddSingleton<TravelRequirementsQuery>()
                .AddSingleton<Mutations.TravelRequirements.SavePassportMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportError_ForNullRequestAttributes()
        {

            var provider = _serviceProvider.GetRequiredService<SavePassportMutation>();

            var result = await provider.SavePassportTravelRequirements(null, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message == "Mandatory field missing." &&
                e.Extensions["title"].ToString() == "Internal server error" &&
                e.Extensions["code"].ToString() == "500")),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportError_ForNullWrongGivenNames()
        {
            var savePassportRequest = new TravelRequirementsSavePassportRequest
            {
                TravelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "Name-mismatch-trm-requiremntId", true, null, null, null, ["01.01|TRM-Wrong-GivenNames"]),
                PassengerId = "01.01|TRM-Wrong-GivenNames",
                IsScanned = false,
                Document = new Models.Document
                {
                    DocumentType = DocumentType.PASSPORT,
                    GivenNames = "Wrong GiveNname",
                    Surname = "TEST",
                    DateOfBirth = "2000-01-01",
                    Gender = new Models.Gender
                    {
                        Code = "M"
                    },
                    ExpirationDate = DateTime.UtcNow.AddYears(5).ToString(),
                    DocumentNumber = "912345678",
                    IssuingCountry = new Models.IssuingCountry
                    {
                        Code = "USA"
                    },
                    Nationality = new Models.Nationality
                    {
                        Code = "USA"
                    },
                    CountryOfResidence = new Models.CountryOfResidence
                    {
                        Code = "USA"
                    }
                }
            };

            var savePassportProvider = _serviceProvider.GetRequiredService<SavePassportMutation>();

            var result = await savePassportProvider.SavePassportTravelRequirements(savePassportRequest, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message.StartsWith("The given name(s) you entered doesn’t match your booking.") &&
                e.Extensions["title"].ToString() == "Something's not quite right" &&
                e.Extensions["code"].ToString() == "travelrequirement_030_002")),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportError_ForExpiredDocument()
        {
            var savePassportRequest = new TravelRequirementsSavePassportRequest
            {
                TravelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "Doc-expired-trm-requiremntId", true, null, null, null, ["01.01|TRM-Wrong-GivenNames"]),
                PassengerId = "01.01|TRM-Expired-Document",
                IsScanned = false,
                Document = new Models.Document
                {
                    DocumentType = DocumentType.PASSPORT,
                    GivenNames = "TRAVELONE",
                    Surname = "TEST",
                    DateOfBirth = "2000-01-01",
                    Gender = new Models.Gender
                    {
                        Code = "M"
                    },
                    ExpirationDate = DateTime.UtcNow.AddYears(-1).ToString(),
                    DocumentNumber = "912345678",
                    IssuingCountry = new Models.IssuingCountry
                    {
                        Code = "USA"
                    },
                    Nationality = new Models.Nationality
                    {
                        Code = "USA"
                    },
                    CountryOfResidence = new Models.CountryOfResidence
                    {
                        Code = "USA"
                    }
                }
            };

            var savePassportProvider = _serviceProvider.GetRequiredService<SavePassportMutation>();

            var result = await savePassportProvider.SavePassportTravelRequirements(savePassportRequest, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message.StartsWith("This passport has expired, so we can’t complete your check in.") &&
                e.Extensions["title"].ToString() == "Something's not quite right" &&
                e.Extensions["code"].ToString() == "travelrequirement_030_006")),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportError_ForInvalidDocumentNumber()
        {
            var savePassportRequest = new TravelRequirementsSavePassportRequest
            {
                TravelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "Doc-invalid-number-trm-requiremntId", true, null, null, null, ["01.01|TRM-Wrong-GivenNames"]),
                PassengerId = "01.01|TRM-Invalid-Document-Number",
                IsScanned = false,
                Document = new Models.Document
                {
                    DocumentType = DocumentType.PASSPORT,
                    GivenNames = "TRAVELONE",
                    Surname = "TEST",
                    DateOfBirth = "2000-01-01",
                    Gender = new Models.Gender
                    {
                        Code = "M"
                    },
                    ExpirationDate = DateTime.UtcNow.AddYears(5).ToString(),
                    DocumentNumber = "912345678%",
                    IssuingCountry = new Models.IssuingCountry
                    {
                        Code = "USA"
                    },
                    Nationality = new Models.Nationality
                    {
                        Code = "USA"
                    },
                    CountryOfResidence = new Models.CountryOfResidence
                    {
                        Code = "USA"
                    }
                }
            };

            var savePassportProvider = _serviceProvider.GetRequiredService<SavePassportMutation>();

            var result = await savePassportProvider.SavePassportTravelRequirements(savePassportRequest, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message.StartsWith("Hmm, we couldn’t recognise that passport number.") &&
                e.Extensions["title"].ToString() == "Something's not quite right" &&
                e.Extensions["code"].ToString() == "travelrequirement_030_005")),
                Times.Once);
        }

        [Fact]
        public async Task Mutation_ShouldReturnResponse_ForGoodRequest()
        {
            var getTravelRequirementsRequest = new TravelRequirementsRequest
            {
                GivenNames = "TRAVELTWO",
                Surname = "TEST",
                PnrLocator = "CNMLQN"
            };

            var getTravelRequirementsProvider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var getTravelRequirementsResponse = await getTravelRequirementsProvider.GetTravelRequirements(getTravelRequirementsRequest, _contextMock.Object);

            var savePassportRequest = new TravelRequirementsSavePassportRequest
            {
                TravelRequirementsId = getTravelRequirementsResponse.TravelRequirementsId,
                PassengerId = getTravelRequirementsResponse.Passengers.First(x => x.GivenNames == "TRAVELTWO").PassengerId,
                IsScanned = false,
                Document = new Models.Document
                {
                    DocumentType = DocumentType.PASSPORT,
                    GivenNames = getTravelRequirementsResponse.Passengers.First(x => x.GivenNames == "TRAVELTWO").GivenNames,
                    Surname = getTravelRequirementsResponse.Passengers.First(x => x.GivenNames == "TRAVELTWO").Surname,
                    DateOfBirth = getTravelRequirementsResponse.Passengers.First(x => x.GivenNames == "TRAVELTWO").DateOfBirth.ToString(),
                    Gender = new Models.Gender
                    {
                        Code = "M"
                    },
                    ExpirationDate = DateTime.UtcNow.AddYears(5).ToString(),
                    DocumentNumber = "925519773",
                    IssuingCountry = new Models.IssuingCountry
                    {
                        Code = "GBR"
                    },
                    Nationality = new Models.Nationality
                    {
                        Code = "GBR"
                    },
                    CountryOfResidence = new Models.CountryOfResidence
                    {
                        Code = "GBR"
                    }
                }
            };

            var savePassportProvider = _serviceProvider.GetRequiredService<SavePassportMutation>();

            var result = await savePassportProvider.SavePassportTravelRequirements(savePassportRequest, _contextMock.Object);

            Assert.NotNull(result);
            Assert.IsType<TravelRequirementsResponse>(result);
            Assert.IsType<string>(result.TravelRequirementsId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));

            //passenger 1, invalid name and expired document
            var passenger1 = result.Passengers.First();
            Assert.Equal("TRAVELONE", passenger1.GivenNames);
            Assert.True(passenger1.Documents.Any());
            Assert.False(passenger1.Documents.First().GivenNames.Is(passenger1.GivenNames));
            Assert.Equal(passenger1.Documents.First().Surname, passenger1.Surname);
            Assert.True(DateTime.Parse(passenger1.Documents.First().ExpirationDate) < DateTime.UtcNow);
            Assert.False(passenger1.SliceRequirements.All(x => x.PassportProvided));
            Assert.False(passenger1.SliceRequirements.All(x => x.PassportVerified));
            Assert.True(passenger1.SliceRequirements.All(x => x.PassportErrors.Any()));
            Assert.True(passenger1.SliceRequirements.All(x => x.UnhappyPassport));
            Assert.True(passenger1.SliceRequirements.Any(x => x.PassportErrors.Any(e => e.Code == "travelrequirement_030_002") && x.PassportErrors.Any(e => e.Code == "travelrequirement_030_006")));
            Assert.True(passenger1.SliceRequirements.Any(x => x.PassportErrors.Any(e => e.DisplayCode == "FirstNameMismatch") && x.PassportErrors.Any(e => e.DisplayCode == "PassportExpiredBeforeDeparture")));

            //passenger 2, right document details provided, UK citizen travelling from UK to US
            var passenger2 = result.Passengers.Skip(1).First();
            Assert.Equal("TRAVELTWO", passenger2.GivenNames);
            Assert.True(passenger2.Documents.Any());
            Assert.True(passenger2.Documents.First().GivenNames.Is(passenger2.GivenNames));
            Assert.Equal(passenger2.Documents.First().Surname, passenger2.Surname);
            Assert.True(DateTime.Parse(passenger2.Documents.First().ExpirationDate) > DateTime.UtcNow);
            Assert.True(passenger2.SliceRequirements.All(x => x.PassportProvided));            
            Assert.False(passenger2.SliceRequirements.All(x => x.UnhappyPassport));

            //slice 1, LHR - JFK
            var psg2Slice1 = passenger2.SliceRequirements.First(x => x.SliceId.Is(result.Slices.First().SliceId));
            Assert.True(psg2Slice1.PassportErrors.Any(e => e.Code == "SecondaryDocETARequirement"));
            Assert.True(psg2Slice1.PassportErrors.Any(e => e.Message.StartsWith("We need to check a few travel documents in person before you can fly. To travel to the U.S.")));

            //slice 2, JFK - LHR
            Assert.False(passenger2.SliceRequirements.First(x => x.SliceId.Is(result.Slices.Skip(1).First().SliceId)).PassportErrors.Any()); 
        }
    }
}
