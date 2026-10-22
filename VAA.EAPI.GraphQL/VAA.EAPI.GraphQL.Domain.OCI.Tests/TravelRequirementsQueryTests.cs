using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class TravelRequirementsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IResolverContext> _contextMock = new();
        public TravelRequirementsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                .AddSingleton<ITravelRequirementsServiceProvider, TravelRequirementsServiceProvider>()
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(source => source.GetRequiredService<TestTelemetryService>())
                .AddLogging()
                .AddSingleton<TravelRequirementsQuery>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Query_ShouldReturnNullAndReportError_ForNullRequestAttributes()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "TravelOne",
                Surname = "Test",
                PnrLocator = null
            };

            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(string.Format(Application.MissingFieldMessage, "PnrLocator")),Times.Once);
        }

        [Fact]
        public async Task Query_ShouldReturnNullAndReportError_ForEmptyRequestAttributes()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "",
                Surname = "Test",
                PnrLocator = "FBQDRA"
            };

            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(string.Format(Application.MissingFieldMessage, "GivenNames")), Times.Once);
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullRequest()
        {
            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(null, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message == "Mandatory field missing." &&
                e.Extensions["title"].ToString() == "Internal server error" &&
                e.Extensions["code"].ToString() == "500")),
                Times.Once);
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForInvalidPNR()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "TravelOne",
                Surname = "TEST",
                PnrLocator = "F12345"
            };

            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.Null(result);

            _contextMock.Verify(x => x.ReportError(It.Is<IError>(e =>
                e.Message == "PNR not found with given details." &&
                e.Extensions["title"].ToString() == "Internal server error" &&
                e.Extensions["code"].ToString() == "500")),
                Times.Once);
        }

        [Fact]
        public async Task Query_ShouldRetainGenericError_WhenCheckInServiceReturnsHttp404()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "TravelOne",
                Surname = "Test",
                PnrLocator = "TRM404"
            };
            var resolverContext = ResolverContextProxy.Create(out var context);
            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("500", error.Extensions["code"]?.ToString());
        }

        [Fact]
        public async Task Query_ShouldReturnDomesticOnlyTravelRequirementsError_WhenCheckInServiceReturns020003()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "TravelOne",
                Surname = "Test",
                PnrLocator = "E3SKJP"
            };
            var resolverContext = ResolverContextProxy.Create(out var context);
            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("travelrequirement_020_003", error.Extensions["code"]?.ToString());
            Assert.Equal(string.Empty, error.Extensions["title"]?.ToString());
            Assert.Equal(
                "Your domestic flight has no special travel requirements. However, government or health requirements may still apply. Please check at the airport.",
                error.Message);

            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            Assert.Equal(2, telemetryService.Events.Count);
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_REQUESTED, telemetryService.Events[0].Type);
            Assert.Equal("E3SKJP", telemetryService.Events[0].Data["PnrLocator"]?.ToString());
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED, telemetryService.Events[1].Type);
            Assert.Equal("E3SKJP", telemetryService.Events[1].Data["PnrLocator"]?.ToString());
            Assert.Contains("travelrequirement_020_003", telemetryService.Events[1].Data["PassportTRMError"]?.ToString());
        }

        [Fact]
        public async Task Query_ShouldRetainInternationalRequirementsForDomesticSegmentAfterShortLayover()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "PSGONECS",
                Surname = "Test",
                PnrLocator = "E3RWWX"
            };
            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.NotNull(result);
            Assert.Single(result.Slices);
            var requirement = Assert.Single(Assert.Single(result.Passengers).SliceRequirements);
            Assert.True(requirement.PassportRequired);
            Assert.Equal("TO_COMPLETE", requirement.Overall.ToString());
            AssertSuccessfulQueryTelemetry("E3RWWX", "short-layover-requirements", "01.01|short-layover-passenger", ("short-layover-slice", true));
        }

        [Fact]
        public async Task Query_ShouldCompleteDomesticSliceAfterLongLayoverIndependently()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "PSGTHREEECD",
                Surname = "Test",
                PnrLocator = "E3SBTE"
            };
            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.NotNull(result);
            Assert.Equal(2, result.Slices.Count());
            var requirements = Assert.Single(result.Passengers).SliceRequirements.ToArray();
            var internationalSlice = Assert.Single(requirements.Where(requirement => requirement.SliceId == "international-slice"));
            var domesticSlice = Assert.Single(requirements.Where(requirement => requirement.SliceId == "domestic-slice"));

            Assert.True(internationalSlice.PassportRequired);
            Assert.Equal("TO_COMPLETE", internationalSlice.Overall.ToString());
            Assert.False(domesticSlice.PassportRequired);
            Assert.Equal("COMPLETE", domesticSlice.Overall.ToString());
            AssertSuccessfulQueryTelemetry(
                "E3SBTE",
                "long-layover-requirements",
                "01.01|long-layover-passenger",
                ("international-slice", true),
                ("domestic-slice", false));
        }

        [Fact]
        public async Task Query_ShouldReturnResponse_ForValidPNR()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "TravelOne",
                Surname = "TEST",
                PnrLocator = "FBQDRA"
            };

            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            Assert.NotNull(result);
            Assert.IsType<TravelRequirementsResponse>(result);
            Assert.IsType<string>(result.TravelRequirementsId);
            Assert.NotNull(await TravelRequirementsIdHelper.GetRequirementsId(result.TravelRequirementsId));
            Assert.Equal("FBQDRA", await TravelRequirementsIdHelper.GetPnrLocator(result.TravelRequirementsId));
            Assert.True(await TravelRequirementsIdHelper.TravelRequirementsIdExpiryTime(result.TravelRequirementsId, "requirementsId"));
            Assert.True(result.ApplyEmergencyContactToAllPax);

            Assert.IsType<Reference>(result.Reference);
            Assert.Equal("FBQDRA", result.Reference.PnrLocator);
            Assert.NotNull(result.Reference.CreatedDate);
            Assert.Equal("2026-05-14", result.Reference.CreatedDate.Value.ToString("yyyy-MM-dd", null));

            Assert.NotNull(result.Passengers);
            Assert.Equal(3, result.Passengers.Count());

            var slices = result.Slices.ToArray();
            Assert.Equal("London Heathrow", slices[0].Destination.CityName);
            Assert.Equal("London Heathrow", slices[0].Segments.Single().Destination.CityName);
            Assert.Equal("Delhi", slices[1].Destination.CityName);
            Assert.Equal("Delhi", slices[1].Segments.Single().Destination.CityName);
            Assert.Equal("New York", slices[2].Destination.CityName);
            Assert.Equal("London Heathrow", slices[2].Segments.First().Destination.CityName);
            Assert.Equal("New York", slices[2].Segments.Last().Destination.CityName);

            var selectedPassengers = await TravelRequirementsIdHelper.GetSelectedPassengers(result.TravelRequirementsId);
            Assert.NotNull(selectedPassengers);
            Assert.Equal(result.Passengers.Select(passenger => passenger.PassengerId), selectedPassengers);
            Assert.True(result.Passengers.All(p => p.Documents.Any()));
            Assert.True(result.Passengers.All(p => p.SliceRequirements.All(s => s.PassportProvided)));
            Assert.IsType<Passenger>(result.Passengers.First());
            Assert.IsType<CheckInRequirements>(result.Passengers.First().SliceRequirements.First());
            Assert.NotNull(result.Passengers.First().SliceRequirements.First().SliceId);

            //passenger1 requirements, British citizen for round trip JFK - LHR - DEL
            var passenger1 = result.Passengers.First();
            Assert.NotNull(passenger1.PassengerId);
            Assert.Equal("TRAVELONE", passenger1.GivenNames);
            Assert.Equal("01", passenger1.PassengerFirstNin);
            Assert.Equal("01", passenger1.PassengerLastNin);
            Assert.NotNull(passenger1.DateOfBirth);
            Assert.True(passenger1.Documents.Any());
            Assert.Equal("GBR", passenger1.Documents.First().Nationality.Code);

            //First Slice  (JFK - LHR)
            var firstSliceRequirement = passenger1.SliceRequirements.First();
            Assert.NotNull(firstSliceRequirement.SliceId);
            Assert.True(firstSliceRequirement.PassportProvided);
            Assert.Equal("NOT_REQUIRED", firstSliceRequirement.ContactTracing.ToString());
            Assert.Equal("NOT_REQUIRED", firstSliceRequirement.ReturnDateStatus.ToString());
            Assert.Equal("NOT_REQUIRED", firstSliceRequirement.SecondaryDocumentStatus.ToString());
            Assert.Equal("PART_COMPLETE", firstSliceRequirement.Overall.ToString());
            Assert.Empty(firstSliceRequirement.PassportErrors);
            var travelRequirementError = Assert.Single(firstSliceRequirement.TravelRequirementErrors);
            Assert.Equal("travelrequirement_030_011", travelRequirementError.Code);
            Assert.Equal("Something's not quite right", travelRequirementError.Title);
            Assert.Equal("Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents.", travelRequirementError.Message);

            //Second Slice  (LHR - DEL) - long layover 
            var secondSliceRequirement = passenger1.SliceRequirements.Skip(1).First();
            Assert.NotNull(secondSliceRequirement.SliceId);
            Assert.True(secondSliceRequirement.PassportProvided);
            Assert.Equal("NOT_REQUIRED", secondSliceRequirement.ContactTracing.ToString());
            Assert.Equal("NOT_REQUIRED", secondSliceRequirement.ReturnDateStatus.ToString());
            Assert.Equal("REQUIRED", secondSliceRequirement.SecondaryDocumentStatus.ToString());
            Assert.Equal("PART_COMPLETE", secondSliceRequirement.Overall.ToString());
            var secondaryDocuments = Assert.IsType<Models.TravelRequirements.SecondaryDocumentRequirement[]>(secondSliceRequirement.SecondaryDocuments);
            var secondaryDocument = Assert.Single(secondaryDocuments);
            Assert.Equal(Models.Enums.DocumentType.TRANSIT_VISA, secondaryDocument.DocumentType);
            Assert.Equal("GB", secondaryDocument.CountryCode);
            Assert.Equal("GBR", secondaryDocument.CountryCode3Letter);
            Assert.Equal("United Kingdom", secondaryDocument.CountryName);
            Assert.Equal("Visa", secondaryDocument.Name);
            Assert.Equal("United Kingdom Visa", secondaryDocument.DisplayName);
            Assert.Equal(Constants.SecondaryDocumentStatus.REQUIRED, secondaryDocument.Status);
            var governmentApplication = Assert.Single(secondaryDocument.SecondaryDocumentApplicationOptions);
            Assert.Equal("EMBASSY_VISA_1", governmentApplication.SecondaryDocumentId);
            Assert.Equal(Models.Enums.ApplicationType.Government, governmentApplication.ApplicationType);
            Assert.Null(governmentApplication.Url);
            Assert.Equal("Visit government website", governmentApplication.UrlText);
            Assert.Equal("Apply at the government embassy", governmentApplication.Title);
            Assert.Equal(
                "Complete your United Kingdom Visa at your closest United Kingdom embassy. Once it's approved, come back here and confirm you're ready to travel.",
                governmentApplication.Message);
            Assert.True(secondSliceRequirement.PassportErrors.Any());
            Assert.Equal("SecondaryDocNonETARequirement", secondSliceRequirement.PassportErrors.First().Code);

            //Third Slice (DEL - JFK)
            var thirdSliceRequirement = passenger1.SliceRequirements.Skip(2).First();
            Assert.NotNull(thirdSliceRequirement.SliceId);
            Assert.True(thirdSliceRequirement.PassportProvided);
            Assert.Equal("COMPLETE", thirdSliceRequirement.ContactTracing.ToString());
            Assert.Equal("REQUIRED", thirdSliceRequirement.ReturnDateStatus.ToString());
            Assert.Equal("REQUIRED", thirdSliceRequirement.SecondaryDocumentStatus.ToString());
            Assert.Equal("PART_COMPLETE", thirdSliceRequirement.Overall.ToString());
            Assert.NotNull(passenger1.ContactTracing);
            Assert.True("3129 DELTA BLVD".Is(passenger1.ContactTracing.StreetAddress) && "55068".Is(passenger1.ContactTracing.Zip));
            Assert.Null(thirdSliceRequirement.ReturnDate);
            Assert.False(thirdSliceRequirement.IsReturnDateDeclined);
            var governmentVisa = Assert.Single(thirdSliceRequirement.SecondaryDocuments);
            Assert.Equal(Models.Enums.DocumentType.VISA, governmentVisa.DocumentType);
            Assert.Equal("US", governmentVisa.CountryCode);
            Assert.Equal("USA", governmentVisa.CountryCode3Letter);
            Assert.Equal("United States Of America", governmentVisa.CountryName);
            Assert.Equal("Visa", governmentVisa.Name);
            Assert.Equal("United States Of America Visa", governmentVisa.DisplayName);
            var governmentVisaApplication = Assert.Single(governmentVisa.SecondaryDocumentApplicationOptions);
            Assert.Equal("EMBASSY_VISA_2", governmentVisaApplication.SecondaryDocumentId);
            Assert.Equal(Models.Enums.ApplicationType.Government, governmentVisaApplication.ApplicationType);
            Assert.Equal("Apply at the government embassy", governmentVisaApplication.Title);
            Assert.Equal(
                "Complete your United States Of America Visa at your closest United States Of America embassy. Once it's approved, come back here and confirm you're ready to travel.",
                governmentVisaApplication.Message);
            Assert.Null(governmentVisaApplication.Url);
            Assert.Equal("Visit government website", governmentVisaApplication.UrlText);

            //passenger3 requirements, Indian citizen for round trip JFK - LHR - DEL
            var passenger3 = result.Passengers.Skip(2).First();
            Assert.NotNull(passenger3.PassengerId);
            Assert.Equal("TRAVELTHREE", passenger3.GivenNames);
            Assert.Equal("03", passenger3.PassengerLastNin);
            Assert.Equal("IND", passenger3.Documents.First().Nationality.Code);
            Assert.NotNull(passenger3.EmergencyContact);
            Assert.True(passenger3.EmergencyContact.FirstName.Is("TEST") && passenger3.EmergencyContact.LastName.Is("CONTACT"));


            //First Slice  (JFK - LHR)
            var firstSliceRequirementPsg3 = passenger3.SliceRequirements.First();
            Assert.Equal("NOT_REQUIRED", firstSliceRequirementPsg3.ContactTracing.ToString());
            Assert.Equal("NOT_REQUIRED", firstSliceRequirementPsg3.ReturnDateStatus.ToString());
            Assert.Equal("REQUIRED", firstSliceRequirementPsg3.SecondaryDocumentStatus.ToString());
            Assert.Equal("PART_COMPLETE", firstSliceRequirementPsg3.Overall.ToString());
            Assert.True(firstSliceRequirementPsg3.PassportErrors.Any());
            Assert.Equal("SecondaryDocNonETARequirement", firstSliceRequirementPsg3.PassportErrors.First().Code);

            //Second Slice  (LHR - DEL) - long layover 
            var secondSliceRequirementPsg3 = passenger3.SliceRequirements.Skip(1).First();
            Assert.Equal("NOT_REQUIRED", secondSliceRequirementPsg3.ContactTracing.ToString());
            Assert.Equal("NOT_REQUIRED", secondSliceRequirementPsg3.ReturnDateStatus.ToString());
            Assert.Equal("NOT_REQUIRED", secondSliceRequirementPsg3.SecondaryDocumentStatus.ToString());

            //Third Slice (DEL - JFK)
            var thirdSliceRequirementPsg3 = passenger3.SliceRequirements.Skip(2).First();
            Assert.Equal("COMPLETE", thirdSliceRequirementPsg3.ContactTracing.ToString());
            Assert.Equal("PROVIDED", thirdSliceRequirementPsg3.ReturnDateStatus.ToString());
            Assert.Equal("REQUIRED", thirdSliceRequirementPsg3.SecondaryDocumentStatus.ToString());
            Assert.Equal("PART_COMPLETE", thirdSliceRequirementPsg3.Overall.ToString());
            Assert.NotNull(passenger3.ContactTracing);
            Assert.NotNull(thirdSliceRequirementPsg3.ReturnDate);
            Assert.Equal("2026-06-01", thirdSliceRequirementPsg3.ReturnDate.Value.ToString("yyyy-MM-dd", null));
        }

        [Fact]
        public async Task Query_ShouldReturnNullDestinationCityName_WhenDestinationAirportIsNotInReferenceData()
        {
            var request = new TravelRequirementsRequest
            {
                GivenNames = "City",
                Surname = "Test",
                PnrLocator = "CITYNULL"
            };

            var provider = _serviceProvider.GetRequiredService<TravelRequirementsQuery>();

            var result = await provider.GetTravelRequirements(request, _contextMock.Object);

            var slice = Assert.Single(result.Slices);
            Assert.Equal("ZZZ", slice.Destination.Code);
            Assert.Null(slice.Destination.CityName);
        }

        private void AssertSuccessfulQueryTelemetry(
            string pnrLocator,
            string requirementsId,
            string passengerId,
            params (string SliceId, bool PassportRequired)[] sliceRequirements)
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            Assert.Equal(2, telemetryService.Events.Count);

            var requestedTelemetry = telemetryService.Events[0];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_REQUESTED, requestedTelemetry.Type);
            Assert.Equal(pnrLocator, requestedTelemetry.Data["PnrLocator"]?.ToString());

            var createdTelemetry = telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_CREATED, createdTelemetry.Type);
            Assert.Equal(pnrLocator, createdTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal(requirementsId, createdTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Contains(passengerId, createdTelemetry.Data["PassengerIds"]?.ToString());

            var sliceDetails = createdTelemetry.Data["SliceDetails"]?.ToString();
            var passengerSliceRequirements = JArray.Parse(createdTelemetry.Data["PassengerSliceRequirements"]?.ToString());
            var loggedSliceRequirements = JArray.Parse(Assert.IsType<string>(passengerSliceRequirements.Single()["SliceRequirements"]?.Value<string>()));

            foreach (var expectedSliceRequirement in sliceRequirements)
            {
                Assert.Contains(expectedSliceRequirement.SliceId, sliceDetails);
                var loggedSliceRequirement = Assert.Single(loggedSliceRequirements.Where(requirement =>
                    requirement["SliceId"]?.Value<string>() == expectedSliceRequirement.SliceId));
                Assert.Equal(expectedSliceRequirement.PassportRequired, loggedSliceRequirement["PassportRequired"]?.Value<bool>());
            }
        }
    }
}
