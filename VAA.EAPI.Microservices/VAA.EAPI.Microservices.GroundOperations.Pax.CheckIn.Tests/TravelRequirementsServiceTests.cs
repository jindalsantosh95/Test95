using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Services;
using Xunit;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public class TravelRequirementsServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public TravelRequirementsServiceTests()
        {
            ConfigReader.Set("DeltaTrmEndpoint", "https://travelrequirementsmgt-api-si.vs01.vs.air4.com/int");
            ConfigReader.Set("DeltaChannelValue", "DFMOBILE");
            ConfigReader.Set("DeltaApiIdValue", "D2");
            ConfigReader.Set("DeltaClientIdValue", "VAMobileAppAIR4_CC_SI");
            ConfigReader.Set("DeltaTrmClientSecret", "TestSecret");
            ConfigReader.Set("DeltaAir4TokenEndpoint", "https://ssaasi.vs01.vs.air4.com/as/token.oauth2");
            ConfigReader.Set("DeltaTestTrmValue", "TestValue");
            ConfigReader.Set("TrmConfigOptionsValue", "TestConfigValue");
            ConfigReader.Set("SherpaAffiliateId", "virginatlantic");
            ConfigReader.Set("SecondaryDocumentContentOverrides", "[{\"countryCode\":\"USA\",\"requirementOption\":\"ETA\",\"displayName\":\"ESTA\",\"governmentTitle\":\"ESTA\",\"governmentUrl\":\"https://esta.cbp.dhs.gov/\"},{\"countryCode\":\"USA\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.state.gov/visas/\"},{\"countryCode\":\"GBR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.gov.uk/eta\"},{\"countryCode\":\"GBR\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.gov.uk/check-uk-visa/\"},{\"countryCode\":\"IND\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://indianvisaonline.gov.in/\"},{\"countryCode\":\"CAN\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.canada.ca/en/immigration-refugees-citizenship/services/visit-canada/eta.html\"},{\"countryCode\":\"CAN\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.canada.ca/en/immigration-refugees-citizenship/services/visit-canada/visitor-visa.html\"},{\"countryCode\":\"JAM\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.enterjamaica.gov.jm/\"},{\"countryCode\":\"KOR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.k-eta.go.kr/\"},{\"countryCode\":\"NGA\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://portal.immigration.gov.ng/\"},{\"countryCode\":\"ZWE\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.evisa.gov.zw/index.html\"},{\"countryCode\":\"ISR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://israel-entry.piba.gov.il/\"},{\"countryCode\":\"IRL\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.visas.inis.gov.ie/\"}]");

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IAir4TokenGenerator, Air4TokenGenerator>()
                .AddSingleton<IDeltaTrmService, DeltaTrmService>()
                .AddSingleton<ITravelRequirementsService, TravelRequirementsService>()
                .AddLogging()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTrmServiceisNull()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<TravelRequirementsService>>();

            Assert.Throws<ArgumentNullException>(() => new TravelRequirementsService(null, logger));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerServiceisNull()
        {
            var deltaTrmService = _serviceProvider.GetRequiredService<IDeltaTrmService>();

            Assert.Throws<ArgumentNullException>(() => new TravelRequirementsService(deltaTrmService, null));
        }

        [Fact]
        public void ReturnOnwardTicketRequirement_ShouldBeReturnedSeparatelyFromPassportErrors()
        {
            var tripRequirement = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "trip-id",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.PASSPORT,
                                Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                                Exceptions =
                                [
                                    new DeltaTrm.Error
                                    {
                                        Code = "travelrequirement_030_011",
                                        Category = DeltaTrm.ErrorCategoryType.VALIDATE,
                                        Message = "Return/onward ticket required."
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };

            var requirements = (CheckInRequirements)(new[] { tripRequirement }, Array.Empty<DeltaTrm.Trip>(), null);
            var error = Assert.Single(requirements.TravelRequirementErrors);

            Assert.True(requirements.PassportProvided);
            Assert.False(requirements.UnhappyPassport);
            Assert.Empty(requirements.PassportErrors);
            Assert.Equal("travelrequirement_030_011", error.Code);
            Assert.Equal("Something's not quite right", error.Title);
            Assert.Equal("Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents.", error.Message);
        }

        [Fact]
        public async Task Service_ShouldThrowNullException_ForNullPnrLocatorAsync()
        {
            var travelRequirementsRequest = new TravelRequirementsRequest
            {
                PnrLocator = null,
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetTravelRequirementsAsync(travelRequirementsRequest));
        }

        [Fact]
        public async Task Service_ShouldThrowException_ForEmptyGivenNamesAsync()
        {
            var travelRequirementsRequest = new TravelRequirementsRequest
            {
                PnrLocator = "TSTPNR",
                GivenNames = "",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await Assert.ThrowsAsync<ArgumentException>(() => provider.GetTravelRequirementsAsync(travelRequirementsRequest));
        }

        [Fact]
        public async Task Service_ShouldThrowNullException_ForNullSurnameAsync()
        {
            var travelRequirementsRequest = new TravelRequirementsRequest
            {
                PnrLocator = "TSTPNR",
                GivenNames = "FlyReady",
                Surname = null
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetTravelRequirementsAsync(travelRequirementsRequest));
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_ReturnsMappedTrmError_ForHttpRequestException()
        {
            var request = new TravelRequirementsRequest
            {
                PnrLocator = "TRM404",
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.GetTravelRequirementsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.Equal("THP4004", Assert.Single(result.Value.Errors).Code);
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_ReturnsMappedTrmError_ForHttpRequestExceptionInvalidPnr()
        {
            var request = new TravelRequirementsRequest
            {
                PnrLocator = "TRM404A",
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.GetTravelRequirementsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.Equal("travelrequirement_011_001", Assert.Single(result.Value.Errors).Code);
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_ReturnsDomesticOnlyTravelRequirementsError()
        {
            var request = new TravelRequirementsRequest
            {
                PnrLocator = "E3SKJP",
                GivenNames = "FlyReady",
                Surname = "Test"
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.GetTravelRequirementsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);

            var error = Assert.Single(result.Value.Errors);
            Assert.Equal("travelrequirement_020_003", error.Code);
            Assert.Equal(string.Empty, error.Title);
            Assert.Equal("Your domestic flight has no special travel requirements. However, government or health requirements may still apply. Please check at the airport.", error.Message);
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_ReturnsMappedHttpStatusCode_ForHttpRequestException()
        {
            var request = new TravelRequirementsRequest
            {
                PnrLocator = "TRM404B",
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.GetTravelRequirementsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.Equal("HTTP_404", Assert.Single(result.Value.Errors).Code);
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_RetainsInternationalRequirementsForDomesticSegmentAfterShortLayover()
        {
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.GetTravelRequirementsAsync(new TravelRequirementsRequest
            {
                PnrLocator = "E3RWWX",
                GivenNames = "PSGONECS",
                Surname = "TEST"
            })).Value.Value;

            // TRM keeps the 7h30 connection in one slice: LHR-JFK (international), JFK-LAS (domestic).
            Assert.Single(result.Slices);
            var passenger = Assert.Single(result.Passengers.Where(passenger => passenger.PassengerId.Is("01.01|93877427-8ca1-4ea9-af56-2cd409e6ddbf")));
            var requirements = Assert.Single(passenger.SliceRequirements);

            Assert.True(requirements.PassportRequired);
            Assert.Equal("TO_COMPLETE", requirements.Overall.ToString());
        }

        [Fact]
        public async Task GetTravelRequirementsAsync_CompletesDomesticSliceAfterLongLayoverIndependently()
        {
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.GetTravelRequirementsAsync(new TravelRequirementsRequest
            {
                PnrLocator = "E3SBTE",
                GivenNames = "PSGTHREEECD",
                Surname = "TEST"
            })).Value.Value;

            // TRM splits the 8h35 connection into an international slice and a domestic-only slice.
            Assert.Equal(2, result.Slices.Count());
            var passenger = Assert.Single(result.Passengers);
            var internationalSlice = Assert.Single(passenger.SliceRequirements.Where(requirements =>
                requirements.SliceId.Contains("36d00e78-d7a6-4e34-b631-a441a96e357c")));
            var domesticSlice = Assert.Single(passenger.SliceRequirements.Where(requirements =>
                requirements.SliceId.Contains("fc388b1f-33d8-440f-a825-23012dbdc4f5")));

            Assert.True(internationalSlice.PassportRequired);
            Assert.Equal("TO_COMPLETE", internationalSlice.Overall.ToString());
            Assert.False(domesticSlice.PassportRequired);
            Assert.Equal("COMPLETE", domesticSlice.Overall.ToString());
        }

        [Fact]
        public void GovernmentFormsMapping_ShouldRemainScopedToApplicableSlice()
        {
            var applicableSlice = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "slice-with-government-forms",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS,
                        Status = new DeltaTrm.DerivedStatus { Code = DeltaTrm.DerivedStatusCodeType.COMPLETE },
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.GOVERNMENT_HEALTH_FORM,
                                Links =
                                [
                                    new DeltaTrm.Link
                                    {
                                        LinkId = "approved-form",
                                        Text = "Approved form",
                                        Href = "https://government.example/form"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };
            var nonApplicableSlice = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "slice-without-government-forms",
                RequirementCategories = []
            };

            var applicable = (CheckInRequirements)(new[] { applicableSlice }, Array.Empty<DeltaTrm.Trip>(), null);
            var nonApplicable = (CheckInRequirements)(new[] { nonApplicableSlice }, Array.Empty<DeltaTrm.Trip>(), null);

            Assert.Equal("slice-with-government-forms", applicable.SliceId);
            var form = Assert.Single(applicable.GovernmentForms!);
            Assert.Equal(Constants.GovernmentFormsStatus.COMPLETED, form.Status);
            Assert.Equal("slice-without-government-forms", nonApplicable.SliceId);
            Assert.Null(nonApplicable.GovernmentForms);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldReturnSeparateTransitAndEntryRequirements()
        {
            var trip = CreateSecondaryDocumentTrip();
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    Id = "transit-visa",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.EMBASSY_VISA],
                    Transit = true,
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("GB", "GBR", "United Kingdom"),
                    Links =
                    [
                        new DeltaTrm.Link
                        {
                            Name = "UK transit visa",
                            Source = "government",
                            Href = "https://government.example/uk-transit"
                        },
                        new DeltaTrm.Link
                        {
                            Source = "vendor",
                            Href = "https://vendor.example/not-used"
                        }
                    ]
                },
                new DeltaTrm.Requirement
                {
                    Id = "entry-visa",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.E_VISA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.PRESENT },
                    Country = Country("US", "USA", "United States"),
                    Links =
                    [
                        new DeltaTrm.Link
                        {
                            Name = "US entry visa",
                            Source = "government",
                            LinkId = "us-government-link",
                            Href = "https://government.example/us-entry"
                        }
                    ]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { trip },
                null,
                Country("IN", "IND", "India"));

            Assert.Equal(Constants.SecondaryDocumentStatus.REQUIRED, requirements.SecondaryDocumentStatus);
            var documents = Assert.IsType<SecondaryDocumentRequirement[]>(requirements.SecondaryDocuments);
            Assert.Equal(2, documents.Length);

            var transitVisa = documents.Single(document => document.DocumentType == Constants.DocumentType.TRANSIT_VISA);
            Assert.Equal(Constants.DocumentType.TRANSIT_VISA, transitVisa.DocumentType);
            Assert.Equal("GBR", transitVisa.CountryCode);
            Assert.Null(transitVisa.CountryName);
            Assert.Equal("UK transit visa", transitVisa.Name);
            Assert.Equal("{countryName} UK transit visa", transitVisa.DisplayName);
            Assert.Equal(Constants.SecondaryDocumentStatus.REQUIRED, transitVisa.Status);
            var governmentApplication = Assert.Single(transitVisa.SecondaryDocumentApplicationOptions);
            Assert.Equal("transit-visa", governmentApplication.SecondaryDocumentId);
            Assert.Equal(Constants.ApplicationType.Government, governmentApplication.ApplicationType);
            Assert.Equal("Apply at the government embassy", governmentApplication.Title);
            Assert.Equal("Complete your {countryName} UK transit visa at your closest {countryName} embassy. Once it's approved, come back here and confirm you're ready to travel.", governmentApplication.Message);
            Assert.Null(governmentApplication.Url);
            Assert.Equal("Visit government website", governmentApplication.UrlText);

            var entryVisa = documents.Single(document => document.DocumentType == Constants.DocumentType.VISA);
            Assert.Equal(Constants.DocumentType.VISA, entryVisa.DocumentType);
            Assert.Equal("USA", entryVisa.CountryCode);
            Assert.Null(entryVisa.CountryName);
            Assert.Equal("US entry visa", entryVisa.Name);
            Assert.Equal("{countryName} US entry visa", entryVisa.DisplayName);
            Assert.Equal(Constants.SecondaryDocumentStatus.COMPLETE, entryVisa.Status);
            Assert.Equal(2, entryVisa.SecondaryDocumentApplicationOptions.Count);
            var entryVisaGovernmentApplication = Assert.Single(entryVisa.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Government);
            Assert.Equal("us-government-link", entryVisaGovernmentApplication.SecondaryDocumentId);
            Assert.Equal(Constants.ApplicationType.Government, entryVisaGovernmentApplication.ApplicationType);
            Assert.Equal("Complete your {countryName} US entry visa through the official government website. Once it's approved, come back here and confirm you're ready to travel.", entryVisaGovernmentApplication.Message);
            Assert.Equal("https://www.state.gov/visas/", entryVisaGovernmentApplication.Url);
            Assert.Equal("Apply on government website", entryVisaGovernmentApplication.UrlText);
            var entryVisaVendorApplication = Assert.Single(entryVisa.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Equal("us-government-link", entryVisaVendorApplication.SecondaryDocumentId);
            Assert.StartsWith("https://apply.joinsherpa.com/travel-restrictions/USA?", entryVisaVendorApplication.Url);
            Assert.DoesNotContain("government.example", entryVisaVendorApplication.Url);
            Assert.Equal("Apply with Sherpa", entryVisaVendorApplication.UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldBuildSherpaLink_ForNonEmbassyTransitVisa()
        {
            var trip = CreateSecondaryDocumentTrip();
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    Id = "transit-visa",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.E_VISA],
                    Transit = true,
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("GB", "GBR", "United Kingdom"),
                    Links =
                    [
                        new DeltaTrm.Link
                        {
                            Name = "UK transit visa",
                            Source = "government",
                            Href = "https://government.example/uk-transit"
                        },
                        new DeltaTrm.Link
                        {
                            Source = "vendor",
                            Href = "https://vendor.example/not-used"
                        }
                    ]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { trip },
                null,
                Country("IN", "IND", "India"));

            var transitVisa = Assert.Single(requirements.SecondaryDocuments!);
            var vendorApplication = Assert.Single(
                transitVisa.SecondaryDocumentApplicationOptions,
                application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Equal("transit-visa", vendorApplication.SecondaryDocumentId);
            Assert.Equal("Apply with Sherpa", vendorApplication.Title);
            Assert.Equal(
                "Sherpa will guide you through the process from start to finish. Once it's approved, come back here and confirm you're ready to travel.",
                vendorApplication.Message);
            Assert.StartsWith("https://apply.joinsherpa.com/travel-restrictions/USA?", vendorApplication.Url);
            Assert.Contains("affiliateId=virginatlantic", vendorApplication.Url);
            Assert.Contains("originCountry=IND", vendorApplication.Url);
            Assert.Contains("originAirport=DEL", vendorApplication.Url);
            Assert.Contains("destinationAirport=JFK", vendorApplication.Url);
            Assert.Contains("nationality=IND", vendorApplication.Url);
            Assert.Contains("departureDate=2026-08-01", vendorApplication.Url);
            Assert.Contains("transitCountries=GBR", vendorApplication.Url);
            Assert.Contains("transitAirports=LHR", vendorApplication.Url);
            Assert.Contains("flightNumber=AI101%2CVS3", vendorApplication.Url);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldExposeVendorApplication_WhenUsEtaHasNoGovernmentLink()
        {
            var trip = CreateSecondaryDocumentTrip();
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    Id = "esta",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.ETA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("US", "USA", "United States"),
                    Links =
                    [
                        new DeltaTrm.Link
                        {
                            Name = "ETA",
                            Source = "vendor",
                            LinkId = "eta-link",
                            Href = "https://vendor.example/not-used"
                        }
                    ]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { trip },
                null,
                Country("GB", "GBR", "United Kingdom"));

            var esta = Assert.Single(requirements.SecondaryDocuments!);
            Assert.Equal(Constants.DocumentType.ETA, esta.DocumentType);
            Assert.Equal("USA", esta.CountryCode);
            Assert.Null(esta.CountryName);
            Assert.Equal("ETA", esta.Name);
            Assert.Equal("ESTA", esta.DisplayName);
            Assert.Equal(2, esta.SecondaryDocumentApplicationOptions.Count);
            var vendorApplication = Assert.Single(esta.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Equal("eta-link", vendorApplication.SecondaryDocumentId);
            Assert.Equal(Constants.ApplicationType.Vendor, vendorApplication.ApplicationType);
            Assert.Equal("Apply with Sherpa", vendorApplication.Title);
            Assert.Equal("Sherpa will guide you through the process from start to finish. Once it's approved, come back here and confirm you're ready to travel.", vendorApplication.Message);
            Assert.DoesNotContain("vendor.example", vendorApplication.Url);
            Assert.Contains("affiliateId=virginatlantic", vendorApplication.Url);
            Assert.Equal("Apply with Sherpa", vendorApplication.UrlText);

            var governmentApplication = Assert.Single(esta.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Government);
            Assert.Equal("eta-link", governmentApplication.SecondaryDocumentId);
            Assert.Equal("ESTA", governmentApplication.Title);
            Assert.Equal(
                "Complete your ESTA through the official government website. Once it's approved, come back here and confirm you're ready to travel.",
                governmentApplication.Message);
            Assert.Equal("https://esta.cbp.dhs.gov/", governmentApplication.Url);
            Assert.Equal("Apply on government website", governmentApplication.UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldUsePrimaryEtaOption_WhenEmbassyVisaIsAlsoPresent()
        {
            var trip = CreateSecondaryDocumentTrip();
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    Id = "esta-with-embassy-alternative",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.ETA, DeltaTrm.RequirementOption.EMBASSY_VISA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("US", "USA", "United States"),
                    Links =
                    [
                        new DeltaTrm.Link
                        {
                            Name = "ETA",
                            Source = "vendor",
                            LinkId = "eta-link"
                        }
                    ]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { trip },
                null,
                Country("GB", "GBR", "United Kingdom"));

            var esta = Assert.Single(requirements.SecondaryDocuments!);
            Assert.True(requirements.SecondaryDocumentStatusForETA);
            Assert.Equal(Constants.DocumentType.ETA, esta.DocumentType);
            Assert.Equal("ESTA", esta.DisplayName);
            Assert.Equal(2, esta.SecondaryDocumentApplicationOptions.Count);

            var vendorApplication = Assert.Single(
                esta.SecondaryDocumentApplicationOptions,
                application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Equal("Apply with Sherpa", vendorApplication.UrlText);

            var governmentApplication = Assert.Single(
                esta.SecondaryDocumentApplicationOptions,
                application => application.ApplicationType == Constants.ApplicationType.Government);
            Assert.Equal("ESTA", governmentApplication.Title);
            Assert.Equal("https://esta.cbp.dhs.gov/", governmentApplication.Url);
            Assert.Equal("Apply on government website", governmentApplication.UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldOnlyTreatFirstEtaOptionForUsIsoAlpha3AsEsta()
        {
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    Id = "eta-not-first",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.E_VISA, DeltaTrm.RequirementOption.ETA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("US", "USA", "United States"),
                    Links = [new DeltaTrm.Link { Name = "ETA", Source = "vendor", LinkId = "eta-not-first" }]
                },
                new DeltaTrm.Requirement
                {
                    Id = "eta-not-usa",
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.ETA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("CA", "CAN", "Canada"),
                    Links = [new DeltaTrm.Link { Name = "ETA", Source = "vendor", LinkId = "eta-not-usa" }]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateSecondaryDocumentTrip() },
                null,
                Country("GB", "GBR", "United Kingdom"));

            Assert.False(requirements.SecondaryDocumentStatusForETA);
            Assert.Equal(2, requirements.SecondaryDocuments.Length);
            Assert.All(requirements.SecondaryDocuments, document => Assert.NotEqual("ESTA", document.DisplayName));
            var usaGovernmentApplication = Assert.Single(
                requirements.SecondaryDocuments.Single(document => document.CountryCode == "USA").SecondaryDocumentApplicationOptions,
                application => application.ApplicationType == Constants.ApplicationType.Government);
            Assert.Equal("Apply on the government website", usaGovernmentApplication.Title);
            Assert.Equal("https://www.state.gov/visas/", usaGovernmentApplication.Url);
            Assert.Equal("Apply on government website", usaGovernmentApplication.UrlText);

            var canadaGovernmentApplication = Assert.Single(
                requirements.SecondaryDocuments.Single(document => document.CountryCode == "CAN").SecondaryDocumentApplicationOptions,
                application => application.ApplicationType == Constants.ApplicationType.Government);
            Assert.Equal("Apply on the government website", canadaGovernmentApplication.Title);
            Assert.Equal(
                "https://www.canada.ca/en/immigration-refugees-citizenship/services/visit-canada/eta.html",
                canadaGovernmentApplication.Url);
            Assert.Equal("Apply on government website", canadaGovernmentApplication.UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldGenerateGovernmentOption_ForVendorOnlyUkTransitVisa()
        {
            var tripRequirement = CreateSecondaryDocumentTripRequirement(new DeltaTrm.Requirement
            {
                RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                RequirementOptions = [DeltaTrm.RequirementOption.EMBASSY_VISA],
                Transit = true,
                Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                Country = Country("GB", "GBR", "United Kingdom"),
                Links = [new DeltaTrm.Link { Name = "UK transit visa", Source = "vendor", LinkId = "uk-transit" }]
            });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateSecondaryDocumentTrip() },
                null,
                Country("IN", "IND", "India"));

            var document = Assert.Single(requirements.SecondaryDocuments);
            Assert.Equal("{countryName} UK transit visa", document.DisplayName);
            var governmentApplication = Assert.Single(document.SecondaryDocumentApplicationOptions);
            Assert.Equal("uk-transit", governmentApplication.SecondaryDocumentId);
            Assert.Equal(Constants.ApplicationType.Government, governmentApplication.ApplicationType);
            Assert.Equal("Apply at the government embassy", governmentApplication.Title);
            Assert.Equal("Complete your {countryName} UK transit visa at your closest {countryName} embassy. Once it's approved, come back here and confirm you're ready to travel.", governmentApplication.Message);
            Assert.Null(governmentApplication.Url);
            Assert.Equal("Visit government website", governmentApplication.UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldGenerateGovernmentOption_ForVendorOnlyElectronicVisa()
        {
            var tripRequirement = CreateSecondaryDocumentTripRequirement(new DeltaTrm.Requirement
            {
                RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                RequirementOptions = [DeltaTrm.RequirementOption.E_VISA],
                Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                Country = Country("IN", "IND", "India"),
                Links = [new DeltaTrm.Link { Name = "Electronic visa", Source = "vendor", LinkId = "india-evisa" }]
            });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateSecondaryDocumentTrip() },
                null,
                Country("GB", "GBR", "United Kingdom"));

            var document = Assert.Single(requirements.SecondaryDocuments);
            Assert.Equal("Electronic visa", document.Name);
            Assert.Equal("{countryName} Electronic visa", document.DisplayName);
            Assert.Equal(2, document.SecondaryDocumentApplicationOptions.Count);
            Assert.All(document.SecondaryDocumentApplicationOptions, application => Assert.Equal("india-evisa", application.SecondaryDocumentId));
            Assert.Single(document.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Equal(
                "Apply with Sherpa",
                Assert.Single(document.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Vendor).UrlText);
            Assert.Equal(
                "https://indianvisaonline.gov.in/",
                Assert.Single(document.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Government).Url);
            Assert.Equal(
                "Apply on government website",
                Assert.Single(document.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Government).UrlText);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldFilterVisaOnArrivalRequirements()
        {
            var tripRequirement = CreateSecondaryDocumentTripRequirement(
                new DeltaTrm.Requirement
                {
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.VISA_ON_ARRIVAL],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("US", "USA", "United States"),
                    Links = [new DeltaTrm.Link { Name = "Visa on arrival", Source = "vendor", LinkId = "on-arrival" }]
                },
                new DeltaTrm.Requirement
                {
                    RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                    RequirementOptions = [DeltaTrm.RequirementOption.E_VISA],
                    Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                    Country = Country("GB", "GBR", "United Kingdom"),
                    Links = [new DeltaTrm.Link { Name = "Electronic visa", Source = "vendor", LinkId = "electronic-visa" }]
                });

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateSecondaryDocumentTrip() },
                null,
                Country("IN", "IND", "India"));

            var document = Assert.Single(requirements.SecondaryDocuments);
            Assert.Equal("electronic-visa", document.SecondaryDocumentApplicationOptions[0].SecondaryDocumentId);
            Assert.Equal("Electronic visa", document.Name);
        }

        [Fact]
        public void SecondaryDocumentsMapping_ShouldReturnNoActionableItems_WhenNoneApply()
        {
            var requirements = (CheckInRequirements)(
                new[] { CreateSecondaryDocumentTripRequirement() },
                new[] { CreateSecondaryDocumentTrip() },
                null,
                Country("GB", "GBR", "United Kingdom"));

            Assert.Equal(Constants.SecondaryDocumentStatus.NOT_REQUIRED, requirements.SecondaryDocumentStatus);
            Assert.Empty(requirements.SecondaryDocuments);
        }

        private static DeltaTrm.PassengerTripHealthRequirement CreateSecondaryDocumentTripRequirement(params DeltaTrm.Requirement[] secondaryDocuments) => new()
        {
            TripId = "secondary-document-trip",
            RequirementCategories =
            [
                new DeltaTrm.RequirementCategory
                {
                    Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                    Requirements =
                    [
                        new DeltaTrm.Requirement
                        {
                            RequirementType = DeltaTrm.RequirementType.PASSPORT,
                            Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.PRESENT },
                            ReturnDateRequired = false
                        },
                        .. secondaryDocuments
                    ]
                }
            ]
        };

        private static DeltaTrm.Trip CreateSecondaryDocumentTrip() => new()
        {
            Id = "secondary-document-trip",
            DepartureStation = Station("DEL", Country("IN", "IND", "India")),
            ArrivalStation = Station("JFK", Country("US", "USA", "United States")),
            FirstDepartureLocalDate = "2026-08-01",
            Segments =
            [
                Segment(1, "AI", "101", Station("DEL", Country("IN", "IND", "India")), Station("LHR", Country("GB", "GBR", "United Kingdom"))),
                Segment(2, "VS", "3", Station("LHR", Country("GB", "GBR", "United Kingdom")), Station("JFK", Country("US", "USA", "United States")))
            ]
        };

        private static DeltaTrm.DatedOperatingSegment Segment(
            int ordinal,
            string carrierCode,
            string flightNumber,
            DeltaTrm.Station origin,
            DeltaTrm.Station destination) => new()
        {
            Ordinal = ordinal,
            MarketingSegment = new DeltaTrm.DatedMarketingSegment
            {
                FlightNum = flightNumber,
                MarketingCarrier = [new DeltaTrm.Carrier { Code = carrierCode }]
            },
            Legs =
            [
                new DeltaTrm.DatedOperatingLeg
                {
                    Ordinal = 1,
                    Departure = new DeltaTrm.TransportDeparture { Station = origin },
                    Arrival = new DeltaTrm.TransportArrival { Station = destination }
                }
            ]
        };

        private static DeltaTrm.Station Station(string code, DeltaTrm.Country country) => new()
        {
            Code = code,
            Country = country
        };

        private static DeltaTrm.Country Country(string code, string isoAlpha3Code, string name) => new()
        {
            Code = code,
            IsoAlpha3Code = isoAlpha3Code,
            Name = name
        };

        [Fact]
        public void GovernmentFormsMapping_ShouldReturnNull_WhenNoUsableLinksExist()
        {
            var tripRequirement = CreateGovernmentFormsTripRequirement(
                DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE);
            tripRequirement.RequirementCategories.Single().Requirements.Single().Links =
            [
                new DeltaTrm.Link
                {
                    Text = "Government form without a URL"
                }
            ];

            var requirements = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateGovernmentFormsTrip() },
                null);

            Assert.Null(requirements.GovernmentForms);
        }

        [Theory]
        [InlineData(DeltaTrm.CalculationFromCodeType.LEG_DEPARTURE, "2099-01-07T12:00:00Z", "Available 72 hours before leg departure")]
        [InlineData(DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE, "2099-01-07T12:00:00Z", "Available 72 hours before departure")]
        [InlineData(DeltaTrm.CalculationFromCodeType.LEG_ARRIVAL, "2099-01-08T20:00:00Z", "Available 72 hours before leg arrival")]
        [InlineData(DeltaTrm.CalculationFromCodeType.TRIP_DESTINATION_ARRIVAL, "2099-01-08T20:00:00Z", "Available 72 hours before arrival")]
        public void GovernmentFormsMapping_ShouldReturnFutureAvailabilityUsingSupportedTripAnchor(
            DeltaTrm.CalculationFromCodeType calculationFromType,
            string expectedAvailableFromUtc,
            string expectedDescription)
        {
            var tripRequirement = CreateGovernmentFormsTripRequirement(calculationFromType);
            var requirements = (CheckInRequirements)(new[] { tripRequirement }, new[] { CreateGovernmentFormsTrip() }, null);

            var form = Assert.Single(requirements.GovernmentForms!);
            Assert.Equal(DateTime.Parse(expectedAvailableFromUtc).ToUniversalTime(), form.AvailableFromUtc);
            Assert.Equal(expectedDescription, form.AvailableFromDescription);
            Assert.False(form.IsAvailable);
        }

        [Fact]
        public void GovernmentFormsMapping_ShouldBeAvailableWithoutAvailabilityMetadata_ForLastDoseWindow()
        {
            var tripRequirement = CreateGovernmentFormsTripRequirement(DeltaTrm.CalculationFromCodeType.LAST_DOSE);
            var requirements = (CheckInRequirements)(new[] { tripRequirement }, new[] { CreateGovernmentFormsTrip() }, null);

            var form = Assert.Single(requirements.GovernmentForms!);
            Assert.Null(form.AvailableFromUtc);
            Assert.Null(form.AvailableFromDescription);
            Assert.True(form.IsAvailable);
        }

        [Fact]
        public void GovernmentFormsMapping_ShouldBeAvailable_WhenAvailableFromUtcIsInThePast()
        {
            var tripRequirement = CreateGovernmentFormsTripRequirement(DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE);
            tripRequirement.RequirementCategories.Single().Requirements.Single().AcceptanceWindow.From = "P100Y";
            var requirements = (CheckInRequirements)(new[] { tripRequirement }, new[] { CreateGovernmentFormsTrip() }, null);

            var form = Assert.Single(requirements.GovernmentForms!);
            Assert.Null(form.AvailableFromUtc);
            Assert.Null(form.AvailableFromDescription);
            Assert.True(form.IsAvailable);
        }

        [Theory]
        [InlineData("P1DT5H6M7S", "Available 1 day 5 hours 6 minutes 7 seconds before departure")]
        [InlineData("PT1H", "Available 1 hour before departure")]
        public void GovernmentFormsMapping_ShouldFormatAvailabilityDescriptionUsingContactTracingPattern(
            string acceptanceWindowFrom,
            string expectedDescription)
        {
            var tripRequirement = CreateGovernmentFormsTripRequirement(DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE);
            tripRequirement.RequirementCategories.Single().Requirements.Single().AcceptanceWindow.From = acceptanceWindowFrom;
            var requirements = (CheckInRequirements)(new[] { tripRequirement }, new[] { CreateGovernmentFormsTrip() }, null);

            var form = Assert.Single(requirements.GovernmentForms!);
            Assert.Equal(expectedDescription, form.AvailableFromDescription);
            Assert.False(form.IsAvailable);
        }

        private static DeltaTrm.PassengerTripHealthRequirement CreateGovernmentFormsTripRequirement(
            DeltaTrm.CalculationFromCodeType calculationFromType) => new()
        {
            TripId = "availability-trip",
            RequirementCategories =
            [
                new DeltaTrm.RequirementCategory
                {
                    Type = DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS,
                    Requirements =
                    [
                        new DeltaTrm.Requirement
                        {
                            AcceptanceWindow = new DeltaTrm.TimeWindow
                            {
                                CalculationFromType = calculationFromType,
                                From = "PT72H"
                            },
                            Links =
                            [
                                new DeltaTrm.Link
                                {
                                    Text = "Government form",
                                    Href = "https://government.example/form"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        private static DeltaTrm.Trip CreateGovernmentFormsTrip() => new()
        {
            Id = "availability-trip",
            Segments =
            [
                new DeltaTrm.DatedOperatingSegment
                {
                    Ordinal = 1,
                    Legs =
                    [
                        new DeltaTrm.DatedOperatingLeg
                        {
                            Ordinal = 1,
                            Departure = new DeltaTrm.TransportDeparture { ScheduledDepartureUTCDateTime = "2099-01-10T12:00:00Z" },
                            Arrival = new DeltaTrm.TransportArrival { ScheduledArrivalUTCDateTime = "2099-01-10T14:00:00Z" }
                        }
                    ]
                },
                new DeltaTrm.DatedOperatingSegment
                {
                    Ordinal = 2,
                    Legs =
                    [
                        new DeltaTrm.DatedOperatingLeg
                        {
                            Ordinal = 1,
                            Departure = new DeltaTrm.TransportDeparture { ScheduledDepartureUTCDateTime = "2099-01-11T16:00:00Z" },
                            Arrival = new DeltaTrm.TransportArrival { ScheduledArrivalUTCDateTime = "2099-01-11T20:00:00Z" }
                        }
                    ]
                }
            ]
        };

        [Fact]
        public async Task Service_ReturnFlightDetails_ForMultiSlicePnrs()
        {
            var travelRequirementsRequest = new TravelRequirementsRequest
            {
                PnrLocator = "CI2HH6",
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.GetTravelRequirementsAsync(travelRequirementsRequest)).Value.Value;

            Assert.NotNull(result);
            Assert.IsType<TravelRequirementsResponse>(result);
            Assert.IsType<NodaTime.LocalDate>(result.Reference.CreatedDate);
            Assert.IsType<Domain.Reference>(result.Reference);
            Assert.Equal("CI2HH6", result.Reference.PnrLocator);
            Assert.Equal("32a96785-22a5-4271-b9e7-d57250f533f1", result.RequirementsId);
            Assert.True(result.Passengers.Any());
            Assert.True(result.Slices.Count() > 1);

            //first slice details
            var firstSlice = result.Slices.FirstOrDefault();
            Assert.IsType<Slice>(firstSlice);
            Assert.NotNull(firstSlice.SliceId);
            Assert.Contains("0f523c76-994a-4bbf-8b22-18146145cdf3", firstSlice.SliceId);
            Assert.NotNull(firstSlice.ScheduledDepartureLocalDateTime);
            Assert.NotNull(firstSlice.ScheduledArrivalLocalDateTime);
            Assert.Equal("LHR", firstSlice.Origin?.Code);
            Assert.Equal("LAS", firstSlice.Destination?.Code);
            Assert.Equal(2, firstSlice.Segments?.Count());

            //first segment of first slice details
            Assert.True("354a3bd7-97d5-4d07-9928-39d448966d8a" == firstSlice.Segments.FirstOrDefault().SegmentId);
            Assert.Equal("Virgin Atlantic", firstSlice.Segments.First().OperatingCarrier.CarrierName);
            Assert.Equal("Virgin Atlantic", firstSlice.Segments.First().MarketingCarrier.CarrierName);
            Assert.Equal("LHR", firstSlice.Segments.First().Origin.Code);
            Assert.Equal("JFK", firstSlice.Segments.First().Destination.Code);
            Assert.Equal("JFK", firstSlice.Segments.First().Legs.First().Destination.Code);

            //second segment of first slice details 
            Assert.True("cd64acc5-b441-4f57-8611-11e49a137d06" == firstSlice.Segments.Skip(1).FirstOrDefault().SegmentId);
            Assert.Equal("Delta Air Lines", firstSlice.Segments.Skip(1).First().OperatingCarrier.CarrierName);
            Assert.Equal("Delta Air Lines", firstSlice.Segments.Skip(1).First().MarketingCarrier.CarrierName);
            Assert.Equal("JFK", firstSlice.Segments.Skip(1).First().Origin.Code);
            Assert.Equal("LAS", firstSlice.Segments.Skip(1).First().Destination.Code);
            Assert.Equal("LAS", firstSlice.Segments.Skip(1).First().Legs.First().Destination.Code);

            //second slice details
            var secondSlice = result.Slices.Skip(1).FirstOrDefault();
            Assert.IsType<Slice>(secondSlice);
            Assert.NotNull(secondSlice.SliceId);
            Assert.True("1003fc30-4c00-411f-af21-26382302f5f4" == secondSlice.Segments.FirstOrDefault().SegmentId);
            Assert.NotNull(secondSlice.ScheduledDepartureLocalDateTime);
            Assert.NotNull(secondSlice.ScheduledArrivalLocalDateTime);
            Assert.Equal("LAS", secondSlice.Origin?.Code);
            Assert.Equal("LHR", secondSlice.Destination?.Code);
            Assert.Equal(1, secondSlice.Segments?.Count());
        }

        [Fact]
        public async Task Service_ShouldReturnRequirements_ForDifferentScenariosAsync()
        {
            var travelRequirementsRequest = new TravelRequirementsRequest
            {
                PnrLocator = "CI2J64",
                GivenNames = "FlyReady",
                Surname = "Test"
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.GetTravelRequirementsAsync(travelRequirementsRequest)).Value.Value;

            Assert.NotNull(result);
            Assert.IsType<TravelRequirementsResponse>(result);
            Assert.Equal("CI2J64", result.Reference.PnrLocator);
            Assert.Equal(3, result.Passengers.Count());
            Assert.IsAssignableFrom<IEnumerable<Domain.Passenger>>(result.Passengers);
            Assert.True(result.ApplyEmergencyContactToAllPax);

            //passenger 1, US citizen flying to USA 
            var passenger1 = result.Passengers.FirstOrDefault(p => p.PassengerId.Is("01.01|c3fa3f38-7574-447f-a630-c6d9bd9ab6d5"));

            Assert.NotNull(passenger1.Documents);
            Assert.IsAssignableFrom<IEnumerable<Domain.Document>>(passenger1.Documents);
            Assert.Equal("USA", passenger1.Documents.FirstOrDefault(x => x.DocumentType == Constants.DocumentType.PASSPORT).Nationality.Code);
            Assert.Equal("USA", passenger1.Documents.FirstOrDefault(x => x.DocumentType == Constants.DocumentType.PASSPORT).CountryOfResidence.Code);
            Assert.Null(passenger1.Requirements);
            Assert.IsType<CheckInRequirements>(passenger1.SliceRequirements.FirstOrDefault());
            Assert.True(passenger1.SliceRequirements.FirstOrDefault().PassportProvided);
            Assert.Equal(Constants.ContactTracingStatus.NOT_REQUIRED, passenger1.SliceRequirements.FirstOrDefault().ContactTracing);
            Assert.Equal(Constants.ReturnDateStatus.NOT_REQUIRED, passenger1.SliceRequirements.FirstOrDefault().ReturnDateStatus);
            Assert.Null(passenger1.SliceRequirements.FirstOrDefault().ReturnDate);
            Assert.Equal(Constants.SecondaryDocumentStatus.NOT_REQUIRED, passenger1.SliceRequirements.FirstOrDefault().SecondaryDocumentStatus);
            Assert.Equal(Constants.OverallCheckInStatus.COMPLETE, passenger1.SliceRequirements.FirstOrDefault().Overall);

            //passenger 2, UK citizen flying to USA 
            var passenger2 = result.Passengers.FirstOrDefault(p => p.PassengerId.Is("02.01|fd00ddad-afbb-4497-9fd5-5a8226cc9e20"));

            Assert.NotNull(passenger2.Documents);
            Assert.IsAssignableFrom<IEnumerable<Domain.Document>>(passenger2.Documents);
            Assert.Equal("GBR", passenger2.Documents.FirstOrDefault(x => x.DocumentType == Constants.DocumentType.PASSPORT).Nationality.Code);
            Assert.Equal("GBR", passenger2.Documents.FirstOrDefault(x => x.DocumentType == Constants.DocumentType.PASSPORT).CountryOfResidence.Code);
            Assert.IsType<CheckInRequirements>(passenger2.SliceRequirements.FirstOrDefault());
            Assert.True(passenger2.SliceRequirements.FirstOrDefault().PassportProvided);
            Assert.Equal(Constants.ContactTracingStatus.TO_COMPLETE, passenger2.SliceRequirements.FirstOrDefault().ContactTracing);
            Assert.Equal(Constants.ReturnDateStatus.REQUIRED, passenger2.SliceRequirements.FirstOrDefault().ReturnDateStatus);
            Assert.Null(passenger2.SliceRequirements.FirstOrDefault().ReturnDate);
            Assert.Equal(Constants.SecondaryDocumentStatus.NOT_REQUIRED, passenger2.SliceRequirements.FirstOrDefault().SecondaryDocumentStatus);
            Assert.Equal(Constants.OverallCheckInStatus.PART_COMPLETE, passenger2.SliceRequirements.FirstOrDefault().Overall);

            //passenger 3, PassportDetails not provided yet
            var passenger3 = result.Passengers.FirstOrDefault(p => p.PassengerId.Is("03.01|fa65d1ba-12c6-49b0-aba1-e748daf4788e"));

            Assert.Equal(0, passenger3.Documents.Count());
            Assert.IsType<CheckInRequirements>(passenger3.SliceRequirements.FirstOrDefault());
            Assert.False(passenger3.SliceRequirements.FirstOrDefault().PassportProvided);
            Assert.Equal(Constants.ReturnDateStatus.PROVIDE_PASSPORT_FIRST, passenger3.SliceRequirements.FirstOrDefault().ReturnDateStatus);
            Assert.Null(passenger3.SliceRequirements.FirstOrDefault().ReturnDate);
            Assert.Equal(Constants.SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, passenger3.SliceRequirements.FirstOrDefault().SecondaryDocumentStatus);
            Assert.Equal(Constants.OverallCheckInStatus.TO_COMPLETE, passenger3.SliceRequirements.FirstOrDefault().Overall);
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnBusinessException_ForInvalidPassportNumber()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5117",
                IsScanned = false,
                PassengerId = "01.01|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "%987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SavePassportAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Value);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.True(result.Value.Errors?.Any() == true);
            Assert.IsType<ErrorDetails>(result.Value.Errors.FirstOrDefault());
            Assert.Equal("travelrequirement_030_005", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("Something's not quite right", result.Value.Errors.FirstOrDefault().Title);
            Assert.StartsWith("Hmm, we couldn’t recognise that passport number.", result.Value.Errors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnBusinessException_ForInvalidFirstName()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "699ae911-f890-4385-954b-a9499ae62d85",
                IsScanned = false,
                PassengerId = "01.01|ca6ca23c-de1a-4afd-85bd-6992c8e0746d",
                Document = new Document
                {
                    DocumentType = Constants.DocumentType.PASSPORT,
                    GivenNames = "$Test(",
                    Surname = "Name",
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SavePassportAsync(request);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.True(result.Value.Errors?.Any() == true);
            Assert.Equal("travelrequirement_030_001", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("Something's not quite right", result.Value.Errors.FirstOrDefault().Title);
            Assert.StartsWith("The names entered don’t match your booking.", result.Value.Errors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnBusinessException_ForInvalidRequirementsId()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "Invalid-Trm-Requirement_Id",
                IsScanned = false,
                PassengerId = "01.01|ca6ca23c-de1a-4afd-85bd-6992c8e0746d",
                Document = new Document
                {
                    DocumentType = Constants.DocumentType.PASSPORT,
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SavePassportAsync(request);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.True(result.Value.Errors?.Any() == true);
            Assert.Equal("travelrequirement_011_001", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("Something went a little off course", result.Value.Errors.FirstOrDefault().Title);
            Assert.StartsWith("We can’t load the travel documents step at the moment.", result.Value.Errors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnBusinessException_ForExpiredPassport()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "abf2fe68-9f20-41c1-9580-0e8b42d63543",
                IsScanned = false,
                PassengerId = "03.01|4e63f0e6-0b0c-4237-9608-e590287764a6",
                Document = new Document
                {
                    GivenNames = "AATRAVELTHREEAA",
                    Surname = "AATESTAA",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "952178713",
                    ExpirationDate = new LocalDate(2023, 4, 12),
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "USA" },
                    CountryOfResidence = new PassportCountry { Code = "USA" },
                    IssuingCountry = new PassportCountry { Code = "USA" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SavePassportAsync(request);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.True(result.Value.Errors?.Any() == true);
            Assert.Equal("travelrequirement_030_006", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("Something's not quite right", result.Value.Errors.FirstOrDefault().Title);
            Assert.StartsWith("This passport has expired,", result.Value.Errors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnRightDetails_ForScannedGermanPassport()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "9bf68270-b083-4c0d-8e39-1f01170a27dg",
                IsScanned = true,
                PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "D" },
                    CountryOfResidence = new PassportCountry { Code = "D" },
                    IssuingCountry = new PassportCountry { Code = "D" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.SavePassportAsync(request)).Value.Value;

            Assert.True(result.Passengers != null && result.Passengers.Any());

            //passenger passport info
            var passengerPassport = result.Passengers.FirstOrDefault(x => x.PassengerId.Is(request.PassengerId)).Documents.FirstOrDefault(x => x.DocumentType == Constants.DocumentType.PASSPORT);
            Assert.Equal("D", passengerPassport.Nationality.Code);
            Assert.Equal("D", passengerPassport.IssuingCountry.Code); 
            Assert.Equal("D", passengerPassport.CountryOfResidence.Code);
            Assert.True(result.Passengers.FirstOrDefault().SliceRequirements.Any());
            Assert.True(result.Passengers.All(x => x.SliceRequirements.All(y => y.PassportProvided == true)));
            Assert.True(result.Passengers.All(x => x.SliceRequirements.All(y => y.PassportVerified == true)));
        }

        [Fact]
        public async Task SavePassportService_ShouldReturnRightDetails_ForGoodRequest()
        {
            var request = new Domain.TravelRequirements.SavePassportRequest
            {
                RequirementsId = "abf2fe68-9f20-41c1-9580-0e8b42d63543",
                IsScanned = false,
                PassengerId = "01.01|d1bb048b-6888-49af-9fc3-81f79ca30567",
                Document = new Document
                {
                    GivenNames = "TRAVELONE",
                    Surname = "TEST",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "952178713",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "IND" },
                    CountryOfResidence = new PassportCountry { Code = "IND" },
                    IssuingCountry = new PassportCountry { Code = "IND" }
                }
            };

            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = (await provider.SavePassportAsync(request));

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<TravelRequirementsResponse>(result.Value.Value);
            Assert.Equal(request.RequirementsId, result.Value.Value.RequirementsId);
            Assert.True(result.Value.Value.Slices.Any());
            Assert.Equal(2, result.Value.Value.Slices.Count());
            Assert.True(result.Value.Value.Passengers.Any());
            Assert.Equal(4, result.Value.Value.Passengers.Count());

            //passenger 1, Indian citizen travelling round trip: LHR - DEL - BOM and BOM - LHR
            var firstPassenger = result.Value.Value.Passengers.First();
            Assert.True(request.PassengerId.Is(firstPassenger.PassengerId));
            Assert.True(firstPassenger.Documents.Any(x => x.DocumentType == Constants.DocumentType.PASSPORT));
            Assert.True(firstPassenger.Documents.First().Nationality.Code.Is("IND") && firstPassenger.Documents.First().CountryOfResidence.Code.Is("IND") && firstPassenger.Documents.First().IssuingCountry.Code.Is("IND"));
            //first slice requirements for first passenger  (LHR - DEL - BOM)
            var psgOneFirstSliceRequirements = firstPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.First().SliceId));
            Assert.True(psgOneFirstSliceRequirements.PassportProvided);
            Assert.False(psgOneFirstSliceRequirements.PassportVerified);  //manual entry so not verified yet
            Assert.True(!psgOneFirstSliceRequirements.PassportErrors.Any());
            Assert.Equal(Constants.SecondaryDocumentStatus.NOT_REQUIRED, psgOneFirstSliceRequirements.SecondaryDocumentStatus);
            Assert.False(psgOneFirstSliceRequirements.UnhappyPassport);
            //second slice requirements for first passenger  (BOM - LHR)
            var psgOneSecondSliceRequirements = firstPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.Last().SliceId));
            Assert.True(psgOneSecondSliceRequirements.PassportErrors.Any());
            Assert.Equal(Constants.SecondaryDocumentStatus.REQUIRED, psgOneSecondSliceRequirements.SecondaryDocumentStatus);
            Assert.True(psgOneSecondSliceRequirements.PassportErrors.Any() && psgOneSecondSliceRequirements.PassportErrors.Count() == 1 && psgOneSecondSliceRequirements.PassportErrors.First().Code.Is("SecondaryDocNonETARequirement"));
            Assert.False(Constants.Application.PassportValidationErrors.Any(e => psgOneSecondSliceRequirements.PassportErrors?.Any(ex => ex.Code == e) == true));
            Assert.False(psgOneSecondSliceRequirements.UnhappyPassport);

            //passenger 2, British citizen travelling round trip: LHR - DEL - BOM and BOM - LHR
            var secondPassenger = result.Value.Value.Passengers.Skip(1).FirstOrDefault();
            Assert.True(secondPassenger.Documents.First().Nationality.Code.Is("GBR") && secondPassenger.Documents.First().CountryOfResidence.Code.Is("GBR") && secondPassenger.Documents.First().IssuingCountry.Code.Is("GBR"));
            //first slice requirements for second passenger  (LHR - DEL - BOM)
            var psgTwoFirstSliceRequirements = secondPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.First().SliceId));
            Assert.True(psgTwoFirstSliceRequirements.PassportProvided);
            Assert.True(psgTwoFirstSliceRequirements.PassportErrors.Any() && Constants.SecondaryDocumentStatus.REQUIRED == psgTwoFirstSliceRequirements.SecondaryDocumentStatus && psgTwoFirstSliceRequirements.PassportErrors.Count() == 1 && psgTwoFirstSliceRequirements.PassportErrors.First().Code.Is("SecondaryDocNonETARequirement"));
            Assert.False(psgTwoFirstSliceRequirements.UnhappyPassport);
            //second slice requirements for second passenger  (BOM - LHR)
            var psgTwoSecondSliceRequirements = secondPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.Last().SliceId));
            Assert.True(psgTwoSecondSliceRequirements.PassportProvided == true && !psgTwoSecondSliceRequirements.PassportErrors.Any());
            Assert.False(psgTwoSecondSliceRequirements.UnhappyPassport);

            //passenger 3, US citizen travelling round trip: LHR - DEL - BOM and BOM - LHR with wrong first and last name and expired passport
            var thirdPassenger = result.Value.Value.Passengers.Skip(2).FirstOrDefault();
            Assert.True(thirdPassenger.Documents.First().Nationality.Code.Is("USA") && thirdPassenger.Documents.First().CountryOfResidence.Code.Is("USA") && thirdPassenger.Documents.First().IssuingCountry.Code.Is("USA"));
            Assert.True(thirdPassenger.Documents.First().ExpirationDate.HasValue && thirdPassenger.Documents.First().ExpirationDate.Value < LocalDate.FromDateTime(DateTime.Today));
            Assert.False(thirdPassenger.Documents.First().GivenNames.Is(thirdPassenger.GivenNames) && thirdPassenger.Documents.First().Surname.Is(thirdPassenger.Surname));
            //first slice requirements for third passenger  (LHR - DEL - BOM)
            var psgThreeFirstSliceRequirements = thirdPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.First().SliceId));
            Assert.True(psgThreeFirstSliceRequirements.PassportErrors.Any() && Constants.SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST == psgThreeFirstSliceRequirements.SecondaryDocumentStatus && psgThreeFirstSliceRequirements.PassportErrors.Count() == 2 && psgThreeFirstSliceRequirements.PassportErrors.First().Code.Is("travelrequirement_030_006") && psgThreeFirstSliceRequirements.PassportErrors.Last().Code.Is("travelrequirement_030_001"));
            Assert.True(Constants.Application.PassportValidationErrors.Any(e => psgThreeFirstSliceRequirements.PassportErrors?.Any(ex => ex.Code == e) == true));
            Assert.True(psgThreeFirstSliceRequirements.UnhappyPassport);
            Assert.False(psgThreeFirstSliceRequirements.PassportProvided);
            //second slice requirements for third passenger  (BOM - LHR)
            var psgThreeSecondSliceRequirements = thirdPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.Last().SliceId));
            Assert.True(psgThreeSecondSliceRequirements.PassportErrors.Any() && Constants.SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST == psgThreeSecondSliceRequirements.SecondaryDocumentStatus && psgThreeSecondSliceRequirements.PassportErrors.Count() == 2 && psgThreeSecondSliceRequirements.PassportErrors.First().Code.Is("travelrequirement_030_006") && psgThreeSecondSliceRequirements.PassportErrors.Last().Code.Is("travelrequirement_030_001"));
            Assert.True(Constants.Application.PassportValidationErrors.Any(e => psgThreeSecondSliceRequirements.PassportErrors?.Any(ex => ex.Code == e) == true));
            Assert.False(psgThreeSecondSliceRequirements.PassportProvided);
            Assert.True(psgThreeSecondSliceRequirements.UnhappyPassport);

            //passenger 4, passport details not provided yet
            var fourthPassenger = result.Value.Value.Passengers.Skip(3).FirstOrDefault();
            var fourthPassengerFirstSliceRequirements = fourthPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.First().SliceId));
            var fourthPassengerLastSliceRequirements = fourthPassenger.SliceRequirements.FirstOrDefault(x => x.SliceId.Is(result.Value.Value.Slices.Last().SliceId));
            Assert.False(fourthPassenger.Documents.Any());
            Assert.Equal(2, fourthPassenger.SliceRequirements.Count());
            Assert.False(fourthPassengerFirstSliceRequirements.PassportProvided);
            Assert.False(fourthPassengerLastSliceRequirements.PassportProvided);
            Assert.Equal(Constants.SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, fourthPassengerFirstSliceRequirements.SecondaryDocumentStatus);
            Assert.Equal(Constants.SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, fourthPassengerLastSliceRequirements.SecondaryDocumentStatus);
            Assert.False(fourthPassengerFirstSliceRequirements.UnhappyPassport);
            Assert.False(fourthPassengerLastSliceRequirements.UnhappyPassport);
        }

        [Fact]
        public async Task SaveReturnDateService_ShouldSendReturnDatePatchAndReturnTravelRequirementsResponse()
        {
            var request = new Domain.TravelRequirements.SaveReturnDateRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                ReturnOn = new LocalDate(2025, 8, 29)
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SaveReturnDateAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Patch &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            Assert.EndsWith($"/requirements/{request.RequirementsId}", patchRequest.RequestUri?.AbsoluteUri);
            Assert.NotNull(patchRequest.Content);

            var payload = await patchRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patch = Assert.Single(document.RootElement.GetProperty("requests").EnumerateArray());

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal("RETURNDATE", patch.GetProperty("using").GetString());
            Assert.Equal("2025-08-29", patch.GetProperty("returnDate").GetString());
            Assert.False(patch.GetProperty("returnDateDeclined").GetBoolean());
            Assert.True(patch.GetProperty("consent").GetBoolean());
            Assert.False(patch.GetProperty("modified").GetBoolean());
            Assert.False(patch.GetProperty("saveToProfile").GetBoolean());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
            Assert.Equal("handcrafted-example-01-trm", result.Value.Value.RequirementsId);
            Assert.Single(result.Value.Value.Passengers);
            Assert.Single(result.Value.Value.Slices);

            var governmentForms = result.Value.Value.Passengers.Single().SliceRequirements.Single().GovernmentForms;
            Assert.NotNull(governmentForms);
            Assert.Collection(
                governmentForms,
                form =>
                {
                    Assert.Equal(Constants.GovernmentFormsStatus.REQUIRED, form.Status);
                    Assert.Equal("2319b7a2-d308-4a49-8985-e3f77de49c57", form.Id);
                    Assert.Equal("AW_EDCard", form.LinkId);
                    Assert.Equal("Aruba ED Card", form.Title);
                    Assert.Equal("https://edcardaruba.aw/", form.Url);
                },
                form =>
                {
                    Assert.Equal(Constants.GovernmentFormsStatus.REQUIRED, form.Status);
                    Assert.Null(form.Id);
                    Assert.Equal("INTLABW_VisaReq", form.LinkId);
                    Assert.Equal("Aruba Visa Requirement Check (Optional Third Party)", form.Title);
                    Assert.Equal("https://apply.joinsherpa.com/products?passport=USA&destinations=ABW&affiliateId=delta&language=en-US", form.Url);
                });
            Assert.Equal(
                Constants.OverallCheckInStatus.PART_COMPLETE,
                result.Value.Value.Passengers.Single().SliceRequirements.Single().Overall);
        }

        [Fact]
        public async Task SaveGovernmentFormsAttestation_ShouldSendScopedAttestationAndReturnRefreshedState()
        {
            var request = new SaveGovernmentFormsAttestationRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                SliceId = "05a6d1e9-a6fc-43a3-9cc0-8c80913863d7",
                Id = "32738f95-5023-4a23-bf8c-16dd388cbaff",
                LinkId = "Maldives Traveller Declaration",
                Text = "Maldives Traveller Declaration",
                Href = "https://imuga.immigration.gov.mv/"
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SaveGovernmentFormsAttestationAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(message => message?.Method == HttpMethod.Patch)!;

            var payload = await patchRequest.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patch = Assert.Single(document.RootElement.GetProperty("requests").EnumerateArray());

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal(request.SliceId, patch.GetProperty("tripId").GetString());
            Assert.Equal("SELF_DECLARATION_FORM", patch.GetProperty("using").GetString());
            Assert.True(patch.GetProperty("consent").GetBoolean());
            Assert.False(patch.GetProperty("modified").GetBoolean());
            Assert.False(patch.GetProperty("saveToProfile").GetBoolean());
            Assert.False(patch.TryGetProperty("identityDocument", out _));
            var documentForm = patch.GetProperty("documentForm");
            Assert.False(documentForm.GetProperty("digitalVerificationEligible").GetBoolean());
            var link = documentForm.GetProperty("link");
            Assert.Equal(request.Id, link.GetProperty("id").GetString());
            Assert.Equal(request.LinkId, link.GetProperty("linkId").GetString());
            Assert.Equal(request.Text, link.GetProperty("text").GetString());
            Assert.Equal(request.Href, link.GetProperty("href").GetString());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(request.RequirementsId, result.Value.Value.RequirementsId);
            var form = Assert.Single(result.Value.Value.Passengers.Single().SliceRequirements.Single().GovernmentForms!);
            Assert.Equal("84cb1c40-466a-4e12-ba3b-d1d552f062ed", form.Id);
            Assert.Equal("INTLABW_VisaReq", form.LinkId);
            Assert.Equal("Aruba Visa Requirement Check (Optional Third Party)", form.Title);
            Assert.Equal("https://apply.joinsherpa.com/products?passport=USA&destinations=ABW&affiliateId=delta&language=en-US", form.Url);
        }

        [Fact]
        public async Task SaveSecondaryDocumentAttestationAsync_ShouldSendSecondaryDocumentPatch()
        {
            var request = new SaveSecondaryDocumentAttestationRequest
            {
                RequirementsId = "secondary-document-requirements-id",
                PassengerId = "01.01|secondary-document-resource-id",
                SliceId = "secondary-document-slice-id",
                SecondaryDocumentId = "ETA_1",
                CountryCode = "US"
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SaveSecondaryDocumentAttestationAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(message => message?.Method == HttpMethod.Patch &&
                    message.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            var payload = await patchRequest.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patch = Assert.Single(document.RootElement.GetProperty("requests").EnumerateArray());
            var documentForm = patch.GetProperty("documentForm");
            var link = documentForm.GetProperty("link");

            Assert.Equal("secondary-document-resource-id", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal(request.SliceId, patch.GetProperty("tripId").GetString());
            Assert.Equal("SECONDARY_DOC", patch.GetProperty("using").GetString());
            Assert.True(patch.GetProperty("consent").GetBoolean());
            Assert.False(documentForm.GetProperty("digitalVerificationEligible").GetBoolean());
            Assert.Equal(request.SecondaryDocumentId, link.GetProperty("linkId").GetString());
            Assert.Single(link.EnumerateObject());
            Assert.Equal(request.CountryCode, patch.GetProperty("country").GetProperty("code").GetString());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("secondary-document-requirements-id", result.Value.Value.RequirementsId);
            var passenger = Assert.Single(result.Value.Value.Passengers);
            Assert.Equal("01.01|4db1e051-37e4-4836-a13b-f921809e3ed4", passenger.PassengerId);
            var sliceRequirements = Assert.Single(passenger.SliceRequirements);
            Assert.Equal("0432af91-b8ce-4f9e-889f-e9d036a1b90b", sliceRequirements.SliceId);
            var secondaryDocument = Assert.Single(sliceRequirements.SecondaryDocuments);
            Assert.Equal(Constants.SecondaryDocumentStatus.COMPLETE, secondaryDocument.Status);
            Assert.NotEqual(Constants.SecondaryDocumentStatus.REQUIRED, secondaryDocument.Status);
            Assert.Equal(Constants.OverallCheckInStatus.PART_COMPLETE, sliceRequirements.Overall);
            Assert.Equal(2, secondaryDocument.SecondaryDocumentApplicationOptions.Count);
            Assert.All(secondaryDocument.SecondaryDocumentApplicationOptions, application => Assert.Equal("ETA_1", application.SecondaryDocumentId));
            Assert.Single(secondaryDocument.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Vendor);
            Assert.Single(secondaryDocument.SecondaryDocumentApplicationOptions, application => application.ApplicationType == Constants.ApplicationType.Government);
        }

        [Fact]
        public async Task SaveReturnDateService_ShouldSendNoReturnDatePatch()
        {
            var request = new Domain.TravelRequirements.SaveReturnDateRequest
            {
                RequirementsId = "handcrafted-example-03-trm",
                PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                NoReturnDate = true
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await provider.SaveReturnDateAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Patch &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            Assert.EndsWith($"/requirements/{request.RequirementsId}", patchRequest.RequestUri?.AbsoluteUri);
            Assert.NotNull(patchRequest.Content);

            var payload = await patchRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patch = Assert.Single(document.RootElement.GetProperty("requests").EnumerateArray());

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal("RETURNDATE", patch.GetProperty("using").GetString());
            Assert.False(patch.TryGetProperty("returnDate", out _));
            Assert.True(patch.GetProperty("returnDateDeclined").GetBoolean());
            Assert.True(patch.GetProperty("consent").GetBoolean());
            Assert.False(patch.GetProperty("modified").GetBoolean());
            Assert.False(patch.GetProperty("saveToProfile").GetBoolean());
        }

        [Fact]
        public async Task SaveReturnDateService_ShouldReturnBusinessException_ForTrmPatchFailure()
        {
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var request = new Domain.TravelRequirements.SaveReturnDateRequest
            {
                RequirementsId = "Invalid-Trm-Requirement_Id",
                PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                ReturnOn = new LocalDate(2025, 8, 29)
            };

            var result = await provider.SaveReturnDateAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Patch &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            Assert.EndsWith($"/requirements/{request.RequirementsId}", patchRequest.RequestUri?.AbsoluteUri);
            Assert.NotNull(patchRequest.Content);

            var payload = await patchRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patch = Assert.Single(document.RootElement.GetProperty("requests").EnumerateArray());

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal("RETURNDATE", patch.GetProperty("using").GetString());
            Assert.Equal("2025-08-29", patch.GetProperty("returnDate").GetString());
            Assert.False(patch.GetProperty("returnDateDeclined").GetBoolean());
            Assert.True(patch.GetProperty("consent").GetBoolean());
            Assert.False(patch.GetProperty("modified").GetBoolean());
            Assert.False(patch.GetProperty("saveToProfile").GetBoolean());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("travelrequirement_011_001", result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldPatchEmergencyContactAndReturnTravelRequirementsState()
        {
            var request = new SaveEmergencyContactRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                EmergencyContacts =
                [
                    new EmergencyContact
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        FirstName = "Jane",
                        LastName = "Helper",
                        PhoneCountryCode = "GB",
                        PhoneNumber = "7700900000",
                        PassengerRefused = false
                    },
                    new EmergencyContact
                    {
                        PassengerId = "02.01|second-passenger-resource-id",
                        FirstName = "Alex",
                        LastName = "Backup",
                        PhoneCountryCode = "US",
                        PhoneNumber = "4045550100",
                        PassengerRefused = false
                    }
                ]
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var convertedRequests = ((DeltaTrm.RequirementFormRequest)request).Requests.ToList();
            Assert.Equal(2, convertedRequests.Count);
            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", convertedRequests[0].PassengerResourceId);
            Assert.Equal("second-passenger-resource-id", convertedRequests[1].PassengerResourceId);
            Assert.All(convertedRequests, convertedRequest =>
            {
                Assert.Equal(DeltaTrm.RequirementType.EMERGENCY_CONTACT, convertedRequest.Using);
                Assert.Single(convertedRequest.EmergencyContact);
                Assert.Null(convertedRequest.ReturnDate);
            });

            var result = await provider.SaveEmergencyContactAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Patch &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
            Assert.EndsWith($"/requirements/{request.RequirementsId}", patchRequest.RequestUri?.AbsoluteUri);
            Assert.NotNull(patchRequest.Content);

            var payload = await patchRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patchedRequirements = document.RootElement.GetProperty("requests").EnumerateArray().ToList();
            Assert.Equal(2, patchedRequirements.Count);

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patchedRequirements[0].GetProperty("passengerResourceId").GetString());
            Assert.Equal("EMERGENCY_CONTACT", patchedRequirements[0].GetProperty("using").GetString());
            Assert.DoesNotContain("passport", patchedRequirements[0].ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.False(patchedRequirements[0].TryGetProperty("returnDate", out _));
            Assert.False(patchedRequirements[0].TryGetProperty("contactTracing", out _));
            var emergencyContact = Assert.Single(patchedRequirements[0].GetProperty("emergencyContact").EnumerateArray());
            Assert.Equal("Jane", emergencyContact.GetProperty("firstName").GetString());
            Assert.Equal("Helper", emergencyContact.GetProperty("lastName").GetString());
            Assert.Equal("GB", emergencyContact.GetProperty("countryCode").GetString());
            Assert.Equal("7700900000", emergencyContact.GetProperty("phoneNumber").GetString());
            Assert.False(emergencyContact.GetProperty("declined").GetBoolean());

            Assert.Equal("second-passenger-resource-id", patchedRequirements[1].GetProperty("passengerResourceId").GetString());
            Assert.Equal("EMERGENCY_CONTACT", patchedRequirements[1].GetProperty("using").GetString());
            Assert.DoesNotContain("passport", patchedRequirements[1].ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.False(patchedRequirements[1].TryGetProperty("returnDate", out _));
            Assert.False(patchedRequirements[1].TryGetProperty("contactTracing", out _));
            var secondEmergencyContact = Assert.Single(patchedRequirements[1].GetProperty("emergencyContact").EnumerateArray());
            Assert.Equal("Alex", secondEmergencyContact.GetProperty("firstName").GetString());
            Assert.Equal("Backup", secondEmergencyContact.GetProperty("lastName").GetString());
            Assert.Equal("US", secondEmergencyContact.GetProperty("countryCode").GetString());
            Assert.Equal("4045550100", secondEmergencyContact.GetProperty("phoneNumber").GetString());
            Assert.False(secondEmergencyContact.GetProperty("declined").GetBoolean());

            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
            Assert.Equal("handcrafted-example-01-trm", result.Value.Value.RequirementsId);
            Assert.True(result.Value.Value.Passengers.Any());
            Assert.True(result.Value.Value.Slices.Any());
            Assert.All(result.Value.Value.Passengers.SelectMany(passenger => passenger.SliceRequirements), requirement => Assert.True(requirement.PassportProvided));
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldThrowNullException_ForNullRequest()
        {
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.SaveEmergencyContactAsync(null));
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldThrowValidationException_ForInvalidPassengerId()
        {
            var request = new SaveEmergencyContactRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                EmergencyContacts =
                [
                    new EmergencyContact
                    {
                        PassengerId = "9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        FirstName = "Jane",
                        LastName = "Helper",
                        PhoneCountryCode = "GB",
                        PhoneNumber = "7700900000",
                        PassengerRefused = false
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            await Assert.ThrowsAsync<ValidationException>(() => provider.SaveEmergencyContactAsync(request));
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldThrowValidationException_ForNullEmergencyContact()
        {
            var request = new SaveEmergencyContactRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                EmergencyContacts = [null]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var exception = await Assert.ThrowsAsync<ValidationException>(() => provider.SaveEmergencyContactAsync(request));
            Assert.Equal("EmergencyContacts cannot contain null entries.", exception.Message);
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldThrowValidationException_ForMissingEmergencyContactDetails()
        {
            var request = new SaveEmergencyContactRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                EmergencyContacts =
                [
                    new EmergencyContact
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        LastName = "Helper",
                        PhoneCountryCode = "GB",
                        PhoneNumber = "7700900000",
                        PassengerRefused = false
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var exception = await Assert.ThrowsAsync<ValidationException>(() => provider.SaveEmergencyContactAsync(request));
            Assert.Equal("EmergencyContacts.FirstName is required.", exception.Message);
        }

        [Fact]
        public async Task SaveEmergencyContactService_ShouldThrowValidationException_ForNonIso2PhoneCountryCode()
        {
            var request = new SaveEmergencyContactRequest
            {
                RequirementsId = "handcrafted-example-01-trm",
                EmergencyContacts =
                [
                    new EmergencyContact
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        FirstName = "Jane",
                        LastName = "Helper",
                        PhoneCountryCode = "44",
                        PhoneNumber = "7700900000",
                        PassengerRefused = false
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var exception = await Assert.ThrowsAsync<ValidationException>(() => provider.SaveEmergencyContactAsync(request));
            Assert.Equal("EmergencyContacts.PhoneCountryCode must be an ISO-2 country code.", exception.Message);
        }

        [Fact]
        public async Task SaveContactTracingService_ShouldThrowValidationException_ForInvalidPassengerId()
        {
            var request = new Domain.TravelRequirements.SaveContactTracingRequest
            {
                RequirementsId = "handcrafted-example-02-trm",
                SliceId = "trip-id-1",
                ContactTracing =
                [
                    new Domain.ContactTracing.ContactTracing
                    {
                        PassengerId = "9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        StreetAddress = "456 Park Ave",
                        City = "New York",
                        State = "NY",
                        Zip = "10001",
                        Email = "traveller@example.test",
                        PrimaryPhoneNumber = "5550100",
                        PrimaryPhoneNumberCountryCode = "1",
                        SecondaryPhoneNumber = "5550101",
                        SecondaryPhoneNumberCountryCode = "1",
                        NoEmail = false,
                        NoSecondaryPhoneNumber = false,
                        InTransit = false
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var exception = await Assert.ThrowsAsync<ValidationException>(() => provider.SaveContactTracingAsync(request));
            Assert.Equal("PassengerId must be in the expected format '<lastNin>.<firstNin>|<trmPassengerResourceId>'.", exception.Message);
        }

        [Fact]
        public async Task SaveContactTracingService_ShouldThrowValidationException_ForMissingContactTracingDetails()
        {
            var request = new Domain.TravelRequirements.SaveContactTracingRequest
            {
                RequirementsId = "handcrafted-example-02-trm",
                SliceId = "trip-id-1",
                ContactTracing =
                [
                    new Domain.ContactTracing.ContactTracing
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        StreetAddress = "456 Park Ave",
                        City = "New York",
                        PrimaryPhoneNumber = "5550100",
                        PrimaryPhoneNumberCountryCode = "1",
                        NoEmail = true,
                        NoSecondaryPhoneNumber = true,
                        InTransit = false
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var exception = await Assert.ThrowsAsync<ValidationException>(() => provider.SaveContactTracingAsync(request));
            Assert.Equal("ContactTracing.State is required.", exception.Message);
        }

        [Fact]
        public async Task SaveContactTracingService_ShouldSendContactTracingPatchAndReturnTravelRequirementsResponse()
        {
            var request = new Domain.TravelRequirements.SaveContactTracingRequest
            {
                RequirementsId = "handcrafted-example-02-trm",
                SliceId = "trip-id-1",
                ContactTracing =
                [
                    new Domain.ContactTracing.ContactTracing
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        StreetAddress = "456 Park Ave",
                        City = "New York",
                        State = "NY",
                        Zip = "10001",
                        Email = "traveller@example.test",
                        PrimaryPhoneNumber = "5550100",
                        PrimaryPhoneNumberCountryCode = "1",
                        SecondaryPhoneNumber = "5550101",
                        SecondaryPhoneNumberCountryCode = "1",
                        NoEmail = false,
                        NoSecondaryPhoneNumber = false,
                        InTransit = false
                    },
                    new Domain.ContactTracing.ContactTracing
                    {
                        PassengerId = "02.01|second-passenger-resource-id",
                        StreetAddress = "789 Test Avenue",
                        City = "Boston",
                        State = "MA",
                        Zip = "02108",
                        Email = "second@example.test",
                        PrimaryPhoneNumber = "5550200",
                        PrimaryPhoneNumberCountryCode = "1",
                        SecondaryPhoneNumber = "5550201",
                        SecondaryPhoneNumberCountryCode = "1",
                        NoEmail = false,
                        NoSecondaryPhoneNumber = false,
                        InTransit = false
                    }
                ]
            };
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SaveContactTracingAsync(request);
            var patchRequest = httpClientFactory.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Patch &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/requirements/{request.RequirementsId}") == true)!;

            Assert.NotNull(patchRequest.Content);

            var payload = await patchRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            var patches = document.RootElement.GetProperty("requests").EnumerateArray().ToArray();
            Assert.Equal(2, patches.Length);
            var patch = patches.Single(patch => patch.GetProperty("passengerResourceId").GetString() == "9d85b4af-0d7c-4095-9a93-62f07a02604d");

            Assert.Equal("9d85b4af-0d7c-4095-9a93-62f07a02604d", patch.GetProperty("passengerResourceId").GetString());
            Assert.Equal("trip-id-1", patch.GetProperty("tripId").GetString());
            Assert.Equal("CONTACT_TRACING", patch.GetProperty("using").GetString());
            Assert.True(patch.GetProperty("consent").GetBoolean());

            var contactInformation = Assert.Single(patch.GetProperty("contactInformation").EnumerateArray());
            var address = contactInformation.GetProperty("address");
            Assert.Equal("456 Park Ave", address.GetProperty("streetAddress").GetString());
            Assert.Equal("USA", address.GetProperty("countryCode").GetString());
            Assert.Equal("New York", address.GetProperty("cityCode").GetString());
            Assert.Equal("NY", address.GetProperty("stateCode").GetString());
            Assert.Equal("10001", address.GetProperty("zipCode").GetString());

            var email = Assert.Single(contactInformation.GetProperty("email").EnumerateArray());
            Assert.Equal("traveller@example.test", email.GetProperty("value").GetString());
            Assert.False(email.GetProperty("declined").GetBoolean());

            var phones = contactInformation.GetProperty("phone").EnumerateArray().ToArray();
            Assert.Equal("5550100", phones.Single(phone => phone.GetProperty("preference").GetString() == "PRIMARY").GetProperty("phoneNumber").GetString());
            Assert.Equal("5550101", phones.Single(phone => phone.GetProperty("preference").GetString() == "SECONDARY").GetProperty("phoneNumber").GetString());

            var secondPatch = patches.Single(patch => patch.GetProperty("passengerResourceId").GetString() == "second-passenger-resource-id");
            Assert.Equal("second-passenger-resource-id", secondPatch.GetProperty("passengerResourceId").GetString());
            Assert.Equal("trip-id-1", secondPatch.GetProperty("tripId").GetString());
            Assert.Equal("CONTACT_TRACING", secondPatch.GetProperty("using").GetString());
            var secondContactInformation = Assert.Single(secondPatch.GetProperty("contactInformation").EnumerateArray());
            Assert.Equal("789 Test Avenue", secondContactInformation.GetProperty("address").GetProperty("streetAddress").GetString());
            Assert.Equal("second@example.test", Assert.Single(secondContactInformation.GetProperty("email").EnumerateArray()).GetProperty("value").GetString());

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(request.RequirementsId, result.Value.Value.RequirementsId);
            Assert.True(result.Value.Value.Passengers.Any());
            Assert.True(result.Value.Value.Slices.Any());
            Assert.False(result.Value.Value.ApplyEmergencyContactToAllPax);
            Assert.Contains(result.Value.Value.Passengers, passenger =>
                passenger.PassengerId == "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d" &&
                passenger.SliceRequirements.Any(requirement => requirement.ContactTracing == Constants.ContactTracingStatus.COMPLETE));
        }

        [Fact]
        public async Task SaveContactTracingService_ShouldReturnBusinessException_ForTrmPatchFailure()
        {
            var request = new Domain.TravelRequirements.SaveContactTracingRequest
            {
                RequirementsId = "Invalid-Trm-Requirement_Id",
                SliceId = "trip-id-1",
                ContactTracing =
                [
                    new Domain.ContactTracing.ContactTracing
                    {
                        PassengerId = "01.01|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                        StreetAddress = "123 Test Street",
                        City = "New York",
                        State = "NY",
                        Zip = "10001",
                        PrimaryPhoneNumber = "5550100",
                        PrimaryPhoneNumberCountryCode = "1",
                        NoEmail = true,
                        NoSecondaryPhoneNumber = true
                    }
                ]
            };
            var provider = _serviceProvider.GetRequiredService<ITravelRequirementsService>();

            var result = await provider.SaveContactTracingAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("travelrequirement_011_001", result.Value.Errors.First().Code);
        }
    }
}
