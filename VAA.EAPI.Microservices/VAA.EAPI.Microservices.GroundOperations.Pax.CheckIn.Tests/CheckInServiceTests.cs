using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Text;
using System.Text.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ContactTracing;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Services;
using Xunit;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public class CheckInServiceTests : IClassFixture<HttpClientFactoryFixture>, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private ICheckInService _checkInService => _serviceProvider.GetRequiredService<ICheckInService>();
        private HttpClientFactoryFixture _httpClientFactoryFixture => _serviceProvider.GetRequiredService<HttpClientFactoryFixture>();

        public CheckInServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("DeltaSsciEndpoint", "https://stg.virginatlantic.com/PCCOciWeb/apiv2");
            ConfigReader.Set("DeltaTrmEndpoint", "https://travelrequirementsmgt-api-si.vs01.vs.air4.com/int");
            ConfigReader.Set("SherpaAffiliateId", "virginatlantic");
            ConfigReader.Set("SecondaryDocumentContentOverrides", "[{\"countryCode\":\"USA\",\"requirementOption\":\"ETA\",\"displayName\":\"ESTA\",\"governmentTitle\":\"ESTA\",\"governmentUrl\":\"https://esta.cbp.dhs.gov/\"},{\"countryCode\":\"USA\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.state.gov/visas/\"},{\"countryCode\":\"GBR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.gov.uk/eta\"},{\"countryCode\":\"GBR\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.gov.uk/check-uk-visa/\"},{\"countryCode\":\"IND\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://indianvisaonline.gov.in/\"},{\"countryCode\":\"CAN\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.canada.ca/en/immigration-refugees-citizenship/services/visit-canada/eta.html\"},{\"countryCode\":\"CAN\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.canada.ca/en/immigration-refugees-citizenship/services/visit-canada/visitor-visa.html\"},{\"countryCode\":\"JAM\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.enterjamaica.gov.jm/\"},{\"countryCode\":\"KOR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://www.k-eta.go.kr/\"},{\"countryCode\":\"NGA\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://portal.immigration.gov.ng/\"},{\"countryCode\":\"ZWE\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.evisa.gov.zw/index.html\"},{\"countryCode\":\"ISR\",\"requirementOption\":\"ETA\",\"governmentUrl\":\"https://israel-entry.piba.gov.il/\"},{\"countryCode\":\"IRL\",\"requirementOption\":\"E_VISA\",\"governmentUrl\":\"https://www.visas.inis.gov.ie/\"}]");
            ConfigReader.Set("DeltaChannelValue", "DFMOBILE");
            ConfigReader.Set("DeltaApiIdValue", "D2");
            ConfigReader.Set("DeltaClientIdValue", "VAMobileAppAIR4_CC_SI");
            ConfigReader.Set("DeltaTrmClientSecret", "TestSecret");
            ConfigReader.Set("DeltaAir4TokenEndpoint", "https://ssaasi.vs01.vs.air4.com/as/token.oauth2");
            ConfigReader.Set("DeltaTestTrmValue", "TestValue");
            ConfigReader.Set("TrmConfigOptionsValue", "TestConfigValue");
            ConfigReader.Set("boardingpassslidingexpiryhours", "48");

            _serviceProvider = new ServiceCollection()
                .AddSingleton(httpClientFactoryFixture)
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IAir4TokenGenerator, Air4TokenGenerator>()
                .AddSingleton<IDeltaTrmService, DeltaTrmService>()
                .AddSingleton<DeltaSsciService>()
                .AddSingleton<IDeltaSsciService>(serviceProvider => serviceProvider.GetRequiredService<DeltaSsciService>())
                .AddSingleton<IDistributedCache, DistributedCache>()
                .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                .AddSingleton<ICheckInService, CheckInService>()
                .AddLogging()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        [Fact]
        public async Task LogTransactionSummaryAsync_MapsRequestToDeltaSsci()
        {
            var request = new TransactionSummaryRequest
            {
                TravelPartyId = "travel-party-id",
                RecordLocator = "PNR123",
                OriginAirportCode = "LHR",
                TransactionError = "redirectJVPartner"
            };

            var isLogged = await _checkInService.LogTransactionSummaryAsync(request);
            var summaryRequest = _httpClientFactoryFixture.Current.GetResponses(ConfigReader.Get("DeltaSsciEndpoint"))
                .Select(response => response.RequestMessage)
                .Single(requestMessage =>
                    requestMessage?.Method == HttpMethod.Post &&
                    requestMessage.RequestUri?.AbsoluteUri.EndsWith($"/checkin/buildTransactionSummary/{request.TravelPartyId}") == true)!;

            Assert.True(isLogged);
            var payload = await summaryRequest.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(payload);
            Assert.Equal(request.RecordLocator, document.RootElement.GetProperty("recordLocator").GetString());
            Assert.Equal(request.OriginAirportCode, document.RootElement.GetProperty("originAirportCode").GetString());
            Assert.Equal(request.TransactionError, document.RootElement.GetProperty("transactionError").GetString());
        }

        [Fact]
        public async Task LogTransactionSummaryAsync_ReturnsFalse_WhenDeltaSsciDoesNotConfirmSuccess()
        {
            var isLogged = await _checkInService.LogTransactionSummaryAsync(new TransactionSummaryRequest
            {
                TravelPartyId = "failed-travel-party",
                RecordLocator = "PNR123",
                OriginAirportCode = "LHR",
                TransactionError = "redirectJVPartner"
            });

            Assert.False(isLogged);
        }

        [Fact]
        public async Task LogTransactionSummaryAsync_ReturnsFalse_WhenRequestIsIncomplete()
        {
            var isLogged = await _checkInService.LogTransactionSummaryAsync(new TransactionSummaryRequest());

            Assert.False(isLogged);
        }


        [Fact]
        public async Task InitiateCheckIn_SuccessAsync()
        {
            var dateTimePattern = LocalDateTimePattern.CreateWithInvariantCulture("s");
            var datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

            var response = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "F747BD",
                OriginCode = "DTW",
                GivenNames = "TATE",
                Surname = "MCRAE"
            });

            var checkInResponse = response.Result.Value.Value;

            // Check In ID
            Assert.NotNull(checkInResponse.TravelPartyId);
            Assert.NotNull(checkInResponse.RequirementsId);
            Assert.Equal("TATE", response.Result.Value.Value.Passengers.FirstOrDefault().GivenNames);
            Assert.Equal("MCRAE", response.Result.Value.Value.Passengers.FirstOrDefault().Surname);

            // Passengers
            Assert.Equal(3, checkInResponse.Passengers.Count());

            var passenger = checkInResponse.Passengers.First();
            Assert.Equal("01.01|21a1ac35-3c4d-4250-9215-f5a5e5d083ca|9962425354350|0d051466-aae5-40d8-8931-6ef0921723be", passenger.PassengerId);
            Assert.Equal("TATE", passenger.GivenNames);
            Assert.Equal("MCRAE", passenger.Surname);
            Assert.Equal("1991-01-01", datePattern.Format(passenger.DateOfBirth));
            Assert.Single(passenger.Documents);
            Assert.True(passenger.Requirements.PassportProvided);
            Assert.False(passenger.Requirements.PassportVerified);
            Assert.Equal("NOT_REQUIRED", passenger.Requirements.ContactTracing.ToString());
            Assert.Equal("COMPLETE", passenger.Requirements.Overall.ToString());

            var passport = passenger.Documents.Single(d => d.DocumentType == Constants.DocumentType.PASSPORT);
            Assert.Equal("241341341", passport.DocumentNumber);
            Assert.Equal("2035-05-04", datePattern.Format(passport.ExpirationDate!.Value));

            var passenger2 = checkInResponse.Passengers.Skip(1).First();
            Assert.Equal("01.02|a6a1ac35-7u6y-8970-9215-f5b56yh083ca|9963467354351|0d051466-aae5-40d8-8931-6ef0921723be", passenger2.PassengerId);
            Assert.Equal("TESTFIVE", passenger2.GivenNames);
            Assert.Equal("ASCASCA", passenger2.Surname);
            Assert.Equal("1981-05-01", datePattern.Format(passenger2.DateOfBirth));
            Assert.Single(passenger2.Documents);
            Assert.True(passenger2.Requirements.PassportProvided);
            Assert.False(passenger2.Requirements.PassportVerified);
            Assert.Equal("NOT_REQUIRED", passenger2.Requirements.ContactTracing.ToString());
            Assert.Equal("COMPLETE", passenger2.Requirements.Overall.ToString());
            Assert.Equal(SecondaryDocumentStatus.NOT_REQUIRED, passenger2.Requirements.SecondaryDocumentStatus);
            Assert.False(passenger2.Requirements.SecondaryDocumentStatusForETA);

            Assert.Equal(ReturnDateStatus.PROVIDE_PASSPORT_FIRST, checkInResponse.Passengers.First(x => x.PassengerId == "01.03|a6a1ac35-7u6y-8970-9215-f5b56yh083ca|996346738900|0d051466-aae5-40d8-8931-6ef0921723ce").Requirements.ReturnDateStatus);
            Assert.Equal(SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, checkInResponse.Passengers.First(x => x.PassengerId == "01.03|a6a1ac35-7u6y-8970-9215-f5b56yh083ca|996346738900|0d051466-aae5-40d8-8931-6ef0921723ce").Requirements.SecondaryDocumentStatus);

            // Current Slice
            Assert.Equal("f7c8b852-4aee-4637-97a5-b980c52c6513", checkInResponse.CurrentSlice.SliceId);

            Assert.Equal("2025-08-04T19:00:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledDepartureLocalDateTime!.Value));
            Assert.Equal("2025-08-04T23:00:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledDepartureUtcDateTime!.Value));
            Assert.Equal("2025-08-05T07:15:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledArrivalLocalDateTime!.Value));
            Assert.Equal("2025-08-05T06:15:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledArrivalUtcDateTime!.Value));

            Assert.Equal("McNamara Terminal", checkInResponse.CurrentSlice.Origin!.TerminalName);
            Assert.Equal("DTW", checkInResponse.CurrentSlice.Origin!.Code);
            Assert.Equal("Detroit", checkInResponse.CurrentSlice.Origin!.AirportName);
            Assert.Equal("USA", checkInResponse.CurrentSlice.Origin!.CountryCode);

            // Delta doesn't send a Terminal for the Destination.
            Assert.Equal("LHR", checkInResponse.CurrentSlice.Destination!.Code);
            Assert.Equal("Heathrow Airport", checkInResponse.CurrentSlice.Destination!.AirportName);
            Assert.Equal("GBR", checkInResponse.CurrentSlice.Destination!.CountryCode);

            // First Segment
            var firstSegment = checkInResponse.CurrentSlice.Segments!.First();
            Assert.Equal("452d0c34-36e0-4cc8-87c0-a8504316e293", firstSegment.SegmentId);
            Assert.Equal("VS", firstSegment.OperatingCarrier.CarrierCode);
            Assert.Equal("Virgin Atlantic", firstSegment.OperatingCarrier.CarrierName);

            // First Leg
            var firstLeg = firstSegment.Legs!.First();
            Assert.Equal("90808b4a-29f5-4628-b1b9-44f09e1b91e9", firstLeg.LegId);
            Assert.Equal("DTW", firstLeg.Origin!.Code);
            Assert.Equal("Detroit", firstLeg.Origin!.AirportName);
            Assert.Equal("USA", firstLeg.Origin!.CountryCode);
            Assert.Equal("LHR", firstLeg.Destination!.Code);
            Assert.Equal("Heathrow Airport", firstLeg.Destination!.AirportName);
            Assert.Equal("GBR", firstLeg.Destination!.CountryCode);
            Assert.Equal("2025-08-04T19:00:00", dateTimePattern.Format(firstLeg.ScheduledDepartureLocalDateTime!.Value));
            Assert.Equal("2025-08-04T23:00:00", dateTimePattern.Format(firstLeg.ScheduledDepartureUtcDateTime!.Value));
            Assert.Equal("2025-08-05T07:15:00", dateTimePattern.Format(firstLeg.ScheduledArrivalLocalDateTime!.Value));
            Assert.Equal("2025-08-05T06:15:00", dateTimePattern.Format(firstLeg.ScheduledArrivalUtcDateTime!.Value));
            Assert.Equal(3753, firstLeg.DistanceMiles);
            Assert.Equal("7H 40M", firstLeg.FlightDuration);

            // Status
            Assert.True(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal(checkInResponse.ShowBaggageAllowance, false);
        }

        [Fact]
        public async Task InitiateCheckIn_MismatchedTrips_Success()
        {
            var dateTimePattern = LocalDateTimePattern.CreateWithInvariantCulture("s");
            var datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

            var response = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "DCHSGQ",
                OriginCode = "DTW",
                GivenNames = "AMIT",
                Surname = "SINGH"
            });

            var checkInResponse = response.Result.Value.Value;

            // Passengers
            Assert.Single(checkInResponse.Passengers);

            var passenger = checkInResponse.Passengers.First();
            Assert.Equal("01.01|1585b43a-6ddc-4274-91e2-d60a9740da96|9999223129462|a08fdf05-7571-436b-8016-ce525c59d293", passenger.PassengerId);
            Assert.Equal("AMIT", passenger.GivenNames);
            Assert.Equal("SINGH", passenger.Surname);
            Assert.Equal("2000-09-15", datePattern.Format(passenger.DateOfBirth));
            Assert.False(passenger.Requirements.PassportProvided);
            Assert.False(passenger.Requirements.PassportVerified);
            Assert.Equal("TO_COMPLETE", passenger.Requirements.ContactTracing.ToString());
            Assert.Equal("TO_COMPLETE", passenger.Requirements.Overall.ToString());

            // Current Slice
            Assert.Equal("99ebefe4-1c09-4d2e-ac38-01c011c0cf2c|e1a9ab37-85e7-4780-aed2-47514a016db4", checkInResponse.CurrentSlice.SliceId);

            Assert.Equal("2025-09-19T09:35:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledDepartureLocalDateTime!.Value));
            Assert.Equal("2025-09-19T08:35:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledDepartureUtcDateTime!.Value));
            Assert.Equal("2025-09-19T22:59:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledArrivalLocalDateTime!.Value));
            Assert.Equal("2025-09-20T02:59:00", dateTimePattern.Format(checkInResponse.CurrentSlice.ScheduledArrivalUtcDateTime!.Value));

            Assert.Equal("Terminal 3", checkInResponse.CurrentSlice.Origin!.TerminalName);
            Assert.Equal("LHR", checkInResponse.CurrentSlice.Origin!.Code);
            Assert.Equal("Heathrow Airport", checkInResponse.CurrentSlice.Origin!.AirportName);
            Assert.Equal("GBR", checkInResponse.CurrentSlice.Origin!.CountryCode);

            // Delta doesn't send a Terminal for the Destination.
            Assert.Equal("YYZ", checkInResponse.CurrentSlice.Destination!.Code);
            Assert.Equal("Lester B. Pearson International Airport", checkInResponse.CurrentSlice.Destination!.AirportName);
            Assert.Equal("CAN", checkInResponse.CurrentSlice.Destination!.CountryCode);

            // First Segment
            var firstSegment = checkInResponse.CurrentSlice.Segments!.First();
            Assert.Equal("31dc5ee5-ef09-4ed7-acbd-4b2ccd237dd6", firstSegment.SegmentId);
            Assert.Equal("VS", firstSegment.OperatingCarrier.CarrierCode);
            Assert.Equal("Virgin Atlantic", firstSegment.OperatingCarrier.CarrierName);
            Assert.Equal("0003", firstSegment.OperatingCarrier.FlightNum);

            // First Segment Only Leg
            var firstLeg = firstSegment.Legs!.Single();
            Assert.Equal("af57545d-db88-4a66-a721-4f3bd37ea071", firstLeg.LegId);
            Assert.Equal("LHR", firstLeg.Origin!.Code);
            Assert.Equal("Heathrow Airport", firstLeg.Origin!.AirportName);
            Assert.Equal("GBR", firstLeg.Origin!.CountryCode);
            Assert.Equal("JFK", firstLeg.Destination!.Code);
            Assert.Equal("John F Kennedy International", firstLeg.Destination!.AirportName);
            Assert.Equal("USA", firstLeg.Destination!.CountryCode);
            Assert.Equal("2025-09-19T09:35:00", dateTimePattern.Format(firstLeg.ScheduledDepartureLocalDateTime!.Value));
            Assert.Equal("2025-09-19T08:35:00", dateTimePattern.Format(firstLeg.ScheduledDepartureUtcDateTime!.Value));
            Assert.Equal("2025-09-19T12:45:00", dateTimePattern.Format(firstLeg.ScheduledArrivalLocalDateTime!.Value));
            Assert.Equal("2025-09-19T16:45:00", dateTimePattern.Format(firstLeg.ScheduledArrivalUtcDateTime!.Value));
            Assert.Equal(3462, firstLeg.DistanceMiles);
            Assert.Equal("8H 10M", firstLeg.FlightDuration);

            // Second Segment
            var secondSegment = checkInResponse.CurrentSlice.Segments!.Last();
            Assert.Equal("5ec44c8c-0eba-4cee-826d-5af4686f3bf2", secondSegment.SegmentId);
            Assert.Equal("9E", secondSegment.OperatingCarrier.CarrierCode);
            Assert.Equal("9E", secondSegment.OperatingCarrier.CarrierName); // 9E is Endeavor Air, but TRM isn't returning that in the response
            Assert.Equal("5170", secondSegment.OperatingCarrier.FlightNum);

            // Second Segment Only Leg
            var secondLeg = secondSegment.Legs!.Single();
            Assert.Equal("3b8964ea-d036-47e4-b704-6ce6e920d1a0", secondLeg.LegId);
            Assert.Equal("JFK", secondLeg.Origin!.Code);
            Assert.Equal("John F Kennedy International", secondLeg.Origin!.AirportName);
            Assert.Equal("USA", secondLeg.Origin!.CountryCode);
            Assert.Equal("YYZ", secondLeg.Destination!.Code);
            Assert.Equal("Lester B. Pearson International Airport", secondLeg.Destination!.AirportName);
            Assert.Equal("CAN", secondLeg.Destination!.CountryCode);
            Assert.Equal("2025-09-19T20:58:00", dateTimePattern.Format(secondLeg.ScheduledDepartureLocalDateTime!.Value));
            Assert.Equal("2025-09-20T00:58:00", dateTimePattern.Format(secondLeg.ScheduledDepartureUtcDateTime!.Value));
            Assert.Equal("2025-09-19T22:59:00", dateTimePattern.Format(secondLeg.ScheduledArrivalLocalDateTime!.Value));
            Assert.Equal("2025-09-20T02:59:00", dateTimePattern.Format(secondLeg.ScheduledArrivalUtcDateTime!.Value));
            Assert.Equal(365, secondLeg.DistanceMiles);
            Assert.Equal("2H 1M", secondLeg.FlightDuration);

            // Status
            Assert.True(checkInResponse.OnlineCheckInStatus.Available);


        }

        [Fact]
        public void InitiateCheckIn_DoNotBoard_ReferredOut()
        {
            var response = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "F75ERN"
            });
            var checkInResponse = response.Result.Value.Value;

            Assert.False(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal("Airport check in required", checkInResponse.OnlineCheckInStatus.Title);
            Assert.Equal("Unfortunately, government regulations require you check in at the airport, either at the kiosk or with an agent, for this flight.", checkInResponse.OnlineCheckInStatus.Message);
        }

        [Fact]
        public void InitiateCheckIn_KlmFirst_ReferredOut()
        {
            var response = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "H7F7VI"
            });
            var checkInResponse = response.Result.Value.Value;

            Assert.False(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal("Your trip starts with KLM", checkInResponse.OnlineCheckInStatus.Title);
            Assert.Equal("We're looking forward to seeing you, but first you've got a flight with KLM. Please head over to the KLM app or airport desks to check in.", checkInResponse.OnlineCheckInStatus.Message);
        }

        [Fact]
        public void InitiateCheckIn_NotFound_ReferredOut()
        {
            var response = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "LHR404"
            });
            var checkInResponse = response.Result.Value.Value;

            Assert.False(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal("Check in unavailable", checkInResponse.OnlineCheckInStatus.Title);
            Assert.Equal("You can't check in for this booking at this time. Please check the booking reference, departure airport and flight status.", checkInResponse.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Success_MockAsync()
        {
            var request = new SavePassportRequest
            {
                TravelPartyId = "handcrafted-example-01",
                RequirementsId = "handcrafted-example-01-trm",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    DocumentType = Constants.DocumentType.PASSPORT,
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<AirportAndTerminal>(result.Value.Value.CurrentSlice.Segments.First().Destination);
            Assert.Equal("USA", result.Value.Value.CurrentSlice.Segments.First().Destination.CountryCode);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Error_InvalidPassportNumber()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5117",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
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

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.Equal("travelrequirement_030_005", result.Value.Value.OnlineCheckInStatus.Code);
            Assert.StartsWith("Hmm, we couldn’t recognise that passport number", result.Value.Value.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task TrmSavePassportDetails_German_IssuingCountryCode()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "9bf68270-b083-4c0d-8e39-1f01170a27cf",
                TravelPartyId = "handcrafted-example-02",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "D" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.NotNull(result.Value.Value.Passengers.First().Documents);
            Assert.Equal("D", result.Value.Value.Passengers.First().Documents.First().IssuingCountry.Code);
        }

        [Fact]
        public async Task TrmSavePassportDetails_German_NationalityCountryCode()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "9bf68270-b083-4c0d-8e39-1f01170a27dg",
                TravelPartyId = "handcrafted-example-02",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "D" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.NotNull(result.Value.Value.Passengers.First().Documents);
            Assert.Equal("D", result.Value.Value.Passengers.First().Documents.First().Nationality.Code);
        }

        [Fact]
        public async Task TrmSavePassportDetails_German_ResidencyCountryCode()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "9bf68270-b083-4c0d-8e39-1f01170a27eh",
                TravelPartyId = "handcrafted-example-02",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "D" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.NotNull(result.Value.Value.Passengers.First().Documents);
            Assert.Equal("D", result.Value.Value.Passengers.First().Documents.First().CountryOfResidence.Code);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Error_InvalidName()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5118",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "?Test",
                    Surname = "Name£",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.Equal("travelrequirement_030_001", result.Value.Value.OnlineCheckInStatus.Code);
            Assert.StartsWith("The names entered don’t match your booking.", result.Value.Value.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Error_ExpiredPassport()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "requirementsid-EGGXER",
                TravelPartyId = "travelpartyid-EGGXER",
                IsScanned = false,
                PassengerId = "01.01|97de5576-ca5d-4914-8ac1-df64fc602d10|9999195374567|87da617a-bd8e-47b8-aafa-3d02c15ca062",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" },
                    ExpirationDate = new LocalDate(2026, 1, 2)
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Value.OnlineCheckInStatus.Available);
            Assert.Equal("travelrequirement_030_006", result.Value.Value.OnlineCheckInStatus.Code);
            Assert.Equal("Something's not quite right", result.Value.Value.OnlineCheckInStatus.Title);
            Assert.Equal("This passport has expired, so we can’t complete your check in. You’ll need to use a valid passport to continue.", result.Value.Value.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Error_DefaultErrorForUndocumentedErrorCode()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5120",
                TravelPartyId = "a61a21d7-1ca3-4d6d-8421-4070e04e297f",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.IsType<OnlineCheckInStatus>(result.Value.Value.OnlineCheckInStatus);
            Assert.Equal("We can't complete check in online", result.Value.Value.OnlineCheckInStatus.Title);
            Assert.Equal("Looks like we've hit a little turbulence. Don't worry, you can still check in at the airport. Our team will have you ready for take-off in no-time.", result.Value.Value.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task TrmSavePassportDetails_Error_RequirementExceptions()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5119",
                TravelPartyId = "a61a21d7-1ca3-4d6d-8421-4070e04e297f",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.NotNull(result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors);
            Assert.IsType<List<APIErrorResponse>>(result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors);
            Assert.StartsWith("This passport has expired,", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault(x => x.Code.Is("travelrequirement_030_006")).Message);
            Assert.Equal("ERROR", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault(x => x.Code.Is("travelrequirement_030_006")).Category);
            Assert.StartsWith("We’re having trouble checking your travel documents right now.", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault(x => x.Code.Is("travelrequirement_030_013")).Message);
            Assert.Equal("ERROR", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault(x => x.Code.Is("travelrequirement_030_013")).Category);
        }

        [Fact]
        public async Task SavePassportAsync_SsciSecondaryDocumentationRequiredUsa()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "784a6dda-e3ed-4ade-9948-bb12529b5119",
                TravelPartyId = "secondarydocs-usa",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault()?.Requirements.PassportErrors;
            Assert.NotNull(passportErrors);
            Assert.Single(passportErrors);
            Assert.Equal("ShowSecondaryDocIsInboundUs", passportErrors.First().Code);
            Assert.Equal("We can't complete check in online", passportErrors.First().Title);
            Assert.Contains("To travel to the U.S., you’ll need a ESTA or other approved entry document.", passportErrors.First().Message);
        }

        [Fact]
        public async Task SavePassportAsync_SsciSecondaryDocumentationRequiredNonUsa()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "f5ceb009-e0fa-4295-9fad-44e55280838e",
                TravelPartyId = "secondarydocs",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|12b62a26-e736-441d-ac43-1610bd030248",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault()?.Requirements.PassportErrors;
            Assert.NotNull(passportErrors);
            Assert.Single(passportErrors);
            Assert.Equal("ShowSecondaryDocIsNotInboundUs", passportErrors.First().Code);
            Assert.Equal("We can't complete check in online", passportErrors.First().Title);
            Assert.StartsWith("Travellers who aren’t citizens need to show a valid visa or entry document.", passportErrors.First().Message);
        }

        [Fact]
        public async Task SavePassportAsync_TrmSecondaryDocumentationRequiredUsa_DoesNotReturnTrmSecondaryDocumentError()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "traveldocs-usa",
                TravelPartyId = "traveldocs-usa",
                IsScanned = false,
                PassengerId = "01.01|ab4d629b-36b8-49ac-b154-6c6afa43ef76|9999197335855|ab4d629b-36b8-49ac-b154-6c6afa43ef76",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault()?.Requirements.PassportErrors;
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocETARequirement") ?? false);
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocNonETARequirement") ?? false);
        }

        [Fact]
        public async Task SavePassportAsync_TrmSecondaryDocumentationRequiredNonUsa_DoesNotReturnTrmSecondaryDocumentError()
        {
            var request = new SavePassportRequest
            {
                RequirementsId = "traveldocs",
                TravelPartyId = "traveldocs",
                IsScanned = false,
                PassengerId = "01.01|ab4d629b-36b8-49ac-b154-6c6afa43ef76|9999197335855|ab4d629b-36b8-49ac-b154-6c6afa43ef76",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault()?.Requirements.PassportErrors;
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocETARequirement") ?? false);
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocNonETARequirement") ?? false);
        }

        [Fact]
        public async Task TrmSaveReturnDate_Success()
        {
            var request = new ReturnDateRequest
            {
                RequirementsId = "handcrafted-example-01-checkin-return-date-trm",
                TravelPartyId = "handcrafted-example-01",
                NoReturnDate = false,
                PassengerId = "01.01|21a1ac35-3c4d-4250-9215-f5a5e5d083ca|43424324234324234324|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                ReturnDate = new DateTime(2025, 8, 29)
            };

            var result = await _checkInService.SaveReturnDateAsync(request);
            var patchRequest = _httpClientFactoryFixture.Current.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
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

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
        }

        [Fact]
        public async Task TrmSaveReturnDate_Declined_Success()
        {
            var request = new ReturnDateRequest
            {
                RequirementsId = "handcrafted-example-01-checkin-no-return-date-trm",
                TravelPartyId = "handcrafted-example-01",
                NoReturnDate = true,
                PassengerId = "01.01|21a1ac35-3c4d-4250-9215-f5a5e5d083ca|43424324234324234324|9d85b4af-0d7c-4095-9a93-62f07a02604d"
            };

            var result = await _checkInService.SaveReturnDateAsync(request);
            var patchRequest = _httpClientFactoryFixture.Current.GetResponses(ConfigReader.Get("DeltaTrmEndpoint"))
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

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.IsType<Response<CheckInResponse>>(result.Value);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
        }

        [Fact]
        public async Task BoardingDocumentResponse_Success_MockAsync()
        {
            var result = await _checkInService.BoardingPassAsync(new BoardingPassRequest
            {
                TravelPartyId = "f5ceb009-e0fa-4295-9fad-44e55280838e"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<List<Domain.BoardingPass.BoardingPass>>(result.Value.Value.BoardingPasses);
            Assert.Equal("TATE", result.Value.Value.BoardingPasses.FirstOrDefault().GivenNames);
            Assert.Equal("MCRAE", result.Value.Value.BoardingPasses.FirstOrDefault().Surname);
            Assert.Equal("79J", result.Value.Value.BoardingPasses.FirstOrDefault().SeatNumber);
            Assert.Equal("F747BD", result.Value.Value.BoardingPasses.FirstOrDefault().PnrLocator);
            Assert.Equal(1, result.Value.Value.BoardingPasses.Count());
            Assert.StartsWith("/9j/4AAQSkZJRgABAgAAAQABAAD/", result.Value.Value.BoardingPasses.FirstOrDefault().QrCode);
            Assert.StartsWith("iVBORw0KGgo", result.Value.Value.BoardingPasses.First().DesktopBoardingPassBase64);
        }

        [Fact]
        public async Task SaveEmergencyContact_Success_MockAsync()
        {
            var checkInResponse = _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND01",
                OriginCode = "LHR"
            }).Result.Value.Value;

            var result = await _checkInService.SaveEmergencyContactDetailsAsync(new Domain.EmergencyContact.EmergencyContactRequest
            {
                RequirementsId = checkInResponse.RequirementsId,
                TravelPartyId = checkInResponse.TravelPartyId,
                ApplyToAllPassengers = false,
                EmergencyContacts = new List<EmergencyContact> { new EmergencyContact
                {
                    PassengerId = "01.01|21a1ac35-3c4d-4250-9215-f5a5e5d083ca|43424324234324234324|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                    FirstName = "TEST",
                    LastName = "NAME",
                    PhoneCountryCode = "US",
                    PhoneNumber = "9876543210",
                    PassengerRefused = false
                } }
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.Passengers.FirstOrDefault().EmergencyContact);
            Assert.Equal("TEST", result.Value.Value.Passengers.FirstOrDefault().EmergencyContact.FirstName);
            Assert.Equal("88323488567890", result.Value.Value.Passengers.FirstOrDefault().EmergencyContact.PhoneNumber);
            Assert.Equal("US", result.Value.Value.Passengers.FirstOrDefault().EmergencyContact.PhoneCountryCode);
        }

        [Fact]
        public async Task BoardingDocumentResponseByPNR_Success_MockAsync()
        {
            var result = await _checkInService.RetrieveBoardingPass(new BoardingPassRequest
            {
                PnrLocator = "F747BD",
                OriginCode = "DTW"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<List<Domain.BoardingPass.BoardingPass>>(result.Value.Value.BoardingPasses);
            Assert.Equal("TATE", result.Value.Value.BoardingPasses.FirstOrDefault().GivenNames);
            Assert.Equal("MCRAE", result.Value.Value.BoardingPasses.FirstOrDefault().Surname);
            Assert.NotNull(result.Value.Value.BoardingPasses.Count());
            Assert.StartsWith("/9j/4AAQSkZJRgABAgAAAQABAAD/", result.Value.Value.BoardingPasses.FirstOrDefault().QrCode);
        }

        [Fact]
        public async Task SaveContactTracing_RequestValidation_FailureCase_MockAsync()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;
            try
            {
                var result = await _checkInService.SaveContactTracingAsync(new Domain.ContactTracing.ContactTracingRequest
                {
                    TravelPartyId = checkInResponse.TravelPartyId,
                    RequirementsId = checkInResponse.RequirementsId,
                    ApplyToAllPassengers = true,
                    SelectedPassengersIds = new List<string> { "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9", "01.02|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6sadc-967b-43df-aa9b-9eb6bde499e9", "01.03|h45730b8-2dde-4434-a22f-18ds6ebf601e|9999219336667|9545fds3c-967b-43df-aa9b-9eb6bde49f45" },
                    ContactTracings = new List<ContactTracing> { new ContactTracing
                {
                    PassengerId = null,
                    StreetAddress = "321 Park Ave",
                    City = "New York",
                    State ="NY",
                    Zip = "10022",
                    Email ="jane.smith@example.com",
                    InTransit =false,
                    NoEmail = false,
                    PrimaryPhoneNumber = "2125551287",
                    PrimaryPhoneNumberCountryCode ="US",
                    NoSecondaryPhoneNumber = true,
                } }
                });
            }
            catch (Exception ex)
            {   // Need to discuss with Jim & Ian for this use case scenario 
                Assert.Equal(ex.Message, "Unexpected character encountered while parsing value: A. Path '', line 0, position 0.");
            }
        }

        [Fact]
        public async Task SaveContactTracing_RequestValidation_SuccessCase_MockAsync()
        {
            var result = await _checkInService.SaveContactTracingAsync(new Domain.ContactTracing.ContactTracingRequest
            {
                TravelPartyId = "handcrafted-example-02",
                RequirementsId = "handcrafted-example-02-trm",
                ApplyToAllPassengers = true,
                SelectedPassengersIds = new List<string> { "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9", "01.02|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6sadc-967b-43df-aa9b-9eb6bde499e9", "01.03|h45730b8-2dde-4434-a22f-18ds6ebf601e|9999219336667|9545fds3c-967b-43df-aa9b-9eb6bde49f45" },
                ContactTracings = new List<ContactTracing> { new ContactTracing
                {
                    PassengerId = null,
                    StreetAddress = "456 Park Ave",
                    City = "New York",
                    State ="NY",
                    Zip = "10022",
                    Email ="jane.smith@example.com",
                    InTransit =false,
                    NoEmail = false,
                    PrimaryPhoneNumber = "2125551287",
                    PrimaryPhoneNumberCountryCode ="US",
                    NoSecondaryPhoneNumber = true,
                } }
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Contains(result.Value.Value.Passengers, p =>
                 p.ContactTracing?.StreetAddress == "456 PARK AVE" &&
                 p.Requirements?.ContactTracing == ContactTracingStatus.COMPLETE);
        }

        [Fact]
        public async Task SelectPassenger_ShouldThrowError_ForNullTravelPartyId()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _checkInService.SelectPassengersAsync(new SelectPassengerRequest { }));
        }

        [Fact]
        public async Task SelectPassenger_ShouldReturn_InfoForSelectedPassenger()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var result = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                PnrLocator = checkInResponse.Reference.PnrLocator,
                FirstName = checkInResponse.Passengers.FirstOrDefault().GivenNames,
                LastName = checkInResponse.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(1).FirstOrDefault().PassengerId,
                    checkInResponse.Passengers.Skip(2).FirstOrDefault().PassengerId
                }
            })).Value.Value;

            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.Equal(3, result.Passengers.Count());
            Assert.NotNull(result.TravelPartyId);
            Assert.NotNull(result.RequirementsId);
            Assert.Equal("handcrafted-example-02", result.TravelPartyId);
            Assert.Equal(checkInResponse.Passengers.FirstOrDefault().PassengerId, result.Passengers.FirstOrDefault().PassengerId);
            Assert.Equal("IAN", result.Passengers.FirstOrDefault().GivenNames);
            Assert.Equal(checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId.Split("|")[2], result.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId.Split("|")[2]);
            Assert.Equal(checkInResponse.RequirementsId, result.RequirementsId);
        }

        [Fact]
        public async Task SelectPassenger_ShouldReturn_InfoForSelectedPassengerevenifPassengerisRemoved()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var firstSelectPassengerResult = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                PnrLocator = checkInResponse.Reference.PnrLocator,
                FirstName = checkInResponse.Passengers.FirstOrDefault().GivenNames,
                LastName = checkInResponse.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(2).FirstOrDefault().PassengerId,
                    checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId
                }
            })).Value.Value;

            var result = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = firstSelectPassengerResult.TravelPartyId,
                PnrLocator = firstSelectPassengerResult.Reference.PnrLocator,
                FirstName = firstSelectPassengerResult.Passengers.FirstOrDefault().GivenNames,
                LastName = firstSelectPassengerResult.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId
                }
            })).Value.Value;

            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.Equal(2, result.Passengers.Count());
            Assert.NotNull(result.TravelPartyId);
            Assert.NotNull(result.RequirementsId);
            Assert.Equal("handcrafted-example-02", result.TravelPartyId);
            Assert.Equal(checkInResponse.Passengers.FirstOrDefault().PassengerId, result.Passengers.FirstOrDefault().PassengerId);
            Assert.Equal("IAN", result.Passengers.FirstOrDefault().GivenNames);
            Assert.Equal(checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId.Split("|")[2], result.Passengers.Skip(1).FirstOrDefault().PassengerId.Split("|")[2]);
            Assert.Equal(checkInResponse.RequirementsId, result.RequirementsId);

        }

        [Fact]
        public async Task SelectPassenger_ShouldReturn_InfoForSelectedPassengerevenIfNewPassengerisAdded()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var firstSelectPassengerResult = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                PnrLocator = checkInResponse.Reference.PnrLocator,
                FirstName = checkInResponse.Passengers.FirstOrDefault().GivenNames,
                LastName = checkInResponse.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(1).FirstOrDefault().PassengerId,
                    checkInResponse.Passengers.Skip(2).FirstOrDefault().PassengerId
                }
            })).Value.Value;

            var result = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = firstSelectPassengerResult.TravelPartyId,
                PnrLocator = firstSelectPassengerResult.Reference.PnrLocator,
                FirstName = firstSelectPassengerResult.Passengers.FirstOrDefault().GivenNames,
                LastName = firstSelectPassengerResult.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId

                }
            })).Value.Value;

            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.Equal(2, result.Passengers.Count());
            Assert.NotNull(result.TravelPartyId);
            Assert.NotNull(result.RequirementsId);
            Assert.Equal(checkInResponse.Passengers.FirstOrDefault().PassengerId, result.Passengers.FirstOrDefault().PassengerId);
            Assert.Equal("IAN", result.Passengers.FirstOrDefault().GivenNames);
            Assert.Null(firstSelectPassengerResult.Passengers.Skip(3).FirstOrDefault());
            Assert.Equal(checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId.Split("|")[2], result.Passengers.Skip(1).FirstOrDefault().PassengerId.Split("|")[2]);
            Assert.Equal(checkInResponse.RequirementsId, result.RequirementsId);
        }

        [Fact]
        public async Task BoardingPassAsync_ShouldFallbackToGetTravelParties_WhenPostSeatsFails()
        {
            var result = await _checkInService.BoardingPassAsync(new BoardingPassRequest
            {
                TravelPartyId = "f5ceb009-e0fa-4295-9fad-44e55280838e",
                PnrLocator = "F747BD"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.IsType<List<Domain.BoardingPass.BoardingPass>>(result.Value.Value.BoardingPasses);
            Assert.Equal("TATE", result.Value.Value.BoardingPasses.FirstOrDefault().GivenNames);
            Assert.Equal("MCRAE", result.Value.Value.BoardingPasses.FirstOrDefault().Surname);
            Assert.Equal(1, result.Value.Value.BoardingPasses.Count());
            Assert.StartsWith("/9j/4AAQSkZJRgABAgAAAQABAAD/", result.Value.Value.BoardingPasses.FirstOrDefault().QrCode);
        }

        [Fact]
        public async Task SaveReturnDateAsync_ShouldReturnSinglePassengerError_NoReturnDateisProvided()
        {
            var result = await _checkInService.SaveReturnDateAsync(new ReturnDateRequest
            {
                TravelPartyId = "handcrafted-example-03",
                RequirementsId = "handcrafted-example-03-trm",
                PassengerId = "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9",
                SelectedPassengersIds = new List<string> { "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9" },
                NoReturnDate = true
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.OnlineCheckInStatus);
            Assert.IsType<List<APIErrorResponse>>(result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors);
            Assert.Equal("NoReturnDateSinglePassenger", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault().Code);
            Assert.Equal("We can't complete check in online", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault().Title);
            Assert.StartsWith("You haven’t provided a confirmed date you’re leaving", result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors.FirstOrDefault().Message);
        }


        [Fact]
        public async Task SaveReturnDateAsync_ShouldReturnMultiplePassengerError_NoReturnDateisProvided()
        {
            var result = await _checkInService.SaveReturnDateAsync(new ReturnDateRequest
            {
                TravelPartyId = "handcrafted-example-03",
                RequirementsId = "handcrafted-example-03-trm",
                PassengerId = "01.01|98863a6a-c7d9-4543-9932-cc85b18e085f|9999219264604|98f6213c-967b-43df-aa9b-9eb6bde499e9",
                SelectedPassengersIds = new List<string> { "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9", "01.02|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6sadc-967b-43df-aa9b-9eb6bde499e9", "01.03|h45730b8-2dde-4434-a22f-18ds6ebf601e|9999219336667|9545fds3c-967b-43df-aa9b-9eb6bde49f45" },
                NoReturnDate = true
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.OnlineCheckInStatus);
            Assert.IsType<List<APIErrorResponse>>(result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors);
            Assert.Equal("NoReturnDateMultiPassenger", result.Value.Value.Passengers.FirstOrDefault(x => x.PassengerId.Is("01.01|98863a6a-c7d9-4543-9932-cc85b18e085f|9999219264604|98f6213c-967b-43df-aa9b-9eb6bde499e9")).Requirements.PassportErrors.FirstOrDefault().Code);
            Assert.Equal("We can't complete check in online", result.Value.Value.Passengers.FirstOrDefault(x => x.PassengerId.Is("01.01|98863a6a-c7d9-4543-9932-cc85b18e085f|9999219264604|98f6213c-967b-43df-aa9b-9eb6bde499e9")).Requirements.PassportErrors.FirstOrDefault().Title);
            Assert.StartsWith("Some guests haven’t provided a confirmed date", result.Value.Value.Passengers.FirstOrDefault(x => x.PassengerId.Is("01.01|98863a6a-c7d9-4543-9932-cc85b18e085f|9999219264604|98f6213c-967b-43df-aa9b-9eb6bde499e9")).Requirements.PassportErrors.FirstOrDefault().Message);
        }

        [Fact]
        public async Task SaveReturnDateAsync_ShouldNotReturnTrmSecondaryDocError_ForVisaRequirements()
        {
            var result = await _checkInService.SaveReturnDateAsync(new ReturnDateRequest
            {
                TravelPartyId = "handcrafted-example-03",
                RequirementsId = "handcrafted-example-03-01-trm",
                PassengerId = "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9",
                SelectedPassengersIds = new List<string> { "01.01|b91730b8-2dde-4434-a22f-17a96ebf601e|9999219336535|98f6213c-967b-43df-aa9b-9eb6bde499e9" },
                NoReturnDate = false
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.OnlineCheckInStatus);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors;
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocETARequirement") ?? false);
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocNonETARequirement") ?? false);
        }

        [Fact]
        public async Task SavePassportAsync_ShouldNotReturnTrmSecondaryDocError_ForVisaRequirements()
        {
            var request = new SavePassportRequest
            {
                TravelPartyId = "handcrafted-example-03",
                RequirementsId = "handcrafted-example-03-02-trm",
                IsScanned = false,
                PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|98f6213c-967b-43df-aa9b-9eb6bde499e9",
                Document = new Document
                {
                    GivenNames = "Test",
                    Surname = "Name",
                    DocumentType = Constants.DocumentType.PASSPORT,
                    DocumentNumber = "987654321",
                    Gender = new Gender { Code = "M" },
                    Nationality = new Nationality { Code = "GBR" },
                    CountryOfResidence = new PassportCountry { Code = "GBR" },
                    IssuingCountry = new PassportCountry { Code = "GBR" }
                }
            };

            var result = await _checkInService.SavePassportAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value.OnlineCheckInStatus);
            var passportErrors = result.Value.Value.Passengers.FirstOrDefault().Requirements.PassportErrors;
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocETARequirement") ?? false);
            Assert.False(passportErrors?.Any(error => error.Code == "SecondaryDocNonETARequirement") ?? false);
        }

        [Fact]
        public async Task SaveContactTracingAsync_ShouldReturnError_MissingCountryCode()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var result = await _checkInService.SaveContactTracingAsync(new Domain.ContactTracing.ContactTracingRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                RequirementsId = checkInResponse.RequirementsId,
                SelectedPassengersIds = checkInResponse.Passengers.Select(psg => psg.PassengerId),
                ApplyToAllPassengers = false,
                ContactTracings = new List<ContactTracing> { new ContactTracing
                {
                    PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|6672652476066|96y1456c-967b-43df-aa9b-9eb6bde711e9",
                    StreetAddress = "14 Park Avenue",
                    City = "New York",
                    State ="NY",
                    Zip = "10022",
                    Email ="jane.smith@example.com",
                    InTransit =false,
                    NoEmail = false,
                    PrimaryPhoneNumber = "2125551287",
                    PrimaryPhoneNumberCountryCode =null,
                    NoSecondaryPhoneNumber = true,
                } }
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.False(result.Value.Value.OnlineCheckInStatus.Available);
            Assert.Equal("THP4009", result.Value.Value.OnlineCheckInStatus.Code);
            Assert.Equal("We can't complete check in online", result.Value.Value.OnlineCheckInStatus.Title);
            Assert.Equal("Looks like we've hit a little turbulence. Don't worry, you can still check in at the airport. Our team will have you ready for take-off in no-time.", result.Value.Value.OnlineCheckInStatus.Message);
        }


        [Fact]
        public async Task SaveContactTracingAsync_ShouldReturnError_ForWrongInvalidTripId()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var result = await _checkInService.SaveContactTracingAsync(new Domain.ContactTracing.ContactTracingRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                RequirementsId = checkInResponse.RequirementsId,
                SelectedPassengersIds = checkInResponse.Passengers.Select(psg => psg.PassengerId),
                ApplyToAllPassengers = false,
                ContactTracings = new List<ContactTracing> { new ContactTracing
                {
                    PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|6672652476066|96y1213c-967b-43df-aa9b-9eb6bde711e9",
                    StreetAddress = "14 Park Avenue",
                    City = "New York",
                    State ="NY",
                    Zip = "10022",
                    Email ="jane.smith@example.com",
                    InTransit =false,
                    PrimaryPhoneNumber = "2125551287",
                    PrimaryPhoneNumberCountryCode = "1",
                } }
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.False(result.Value.Value.OnlineCheckInStatus.Available);
            Assert.Equal("THP5004", result.Value.Value.OnlineCheckInStatus.Code);
            Assert.Equal("We can't complete check in online", result.Value.Value.OnlineCheckInStatus.Title);
            Assert.Equal("Looks like we've hit a little turbulence. Don't worry, you can still check in at the airport. Our team will have you ready for take-off in no-time.", result.Value.Value.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task SaveContactTracingAsync_ShouldDeclineEmailandSecondaryContact_IfNotProvided()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var result = await _checkInService.SaveContactTracingAsync(new Domain.ContactTracing.ContactTracingRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                RequirementsId = checkInResponse.RequirementsId,
                SelectedPassengersIds = checkInResponse.Passengers.Select(psg => psg.PassengerId),
                ApplyToAllPassengers = false,
                ContactTracings = new List<ContactTracing> { new ContactTracing
                {
                    PassengerId = "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|9d85b4af-0d7c-4095-9a93-62f07a02604d",
                    StreetAddress = "456 Park Ave",
                    City = "New York",
                    State ="NY",
                    Zip = "10022",
                    Email = null,
                    InTransit =false,
                    PrimaryPhoneNumber = "2125551287",
                    PrimaryPhoneNumberCountryCode = "1",
                    SecondaryPhoneNumber = null,
                    SecondaryPhoneNumberCountryCode = null
                } }
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.True(result.Value.Value.OnlineCheckInStatus.Available);
            Assert.NotNull(result.Value.Value.TravelPartyId);
            Assert.NotNull(result.Value.Value.RequirementsId);
            Assert.NotNull(result.Value.Value.Passengers.First(x => x.PassengerId == "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|9d85b4af-0d7c-4095-9a93-62f07a02604d").ContactTracing);
            Assert.True(result.Value.Value.Passengers.First(x => x.PassengerId == "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|9d85b4af-0d7c-4095-9a93-62f07a02604d").ContactTracing.NoEmail);
            Assert.True(result.Value.Value.Passengers.First(x => x.PassengerId == "01.01|bd936308-05c3-4432-bfd4-c06087522f46|9962652476066|9d85b4af-0d7c-4095-9a93-62f07a02604d").ContactTracing.NoSecondaryPhoneNumber);
        }

        [Fact]
        public async Task GetBoardingPassByPNRAsync_ShouldReturnQRResposne_ForPartiallyCheckedInPsgsInPnr()
        {
            var result = await _checkInService.RetrieveBoardingPass(new BoardingPassRequest
            {
                OriginCode = "LHR",
                PnrLocator = "HAND02"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PassengersCheckInStatus.PARTIALLY_COMPLETE, result.Value.Value.CheckInStatus);
            Assert.Equal(6, result.Value.Value.BoardingPasses.Count());
            Assert.NotNull(result.Value.Value.BoardingPasses.FirstOrDefault(x => x.CheckedIn).QrCode);
            Assert.Null(result.Value.Value.BoardingPasses.FirstOrDefault(x => !x.CheckedIn).QrCode);
            Assert.Equal(2, result.Value.Value.BoardingPasses.Count(x => x.CheckedIn));
            Assert.Equal(
                "ZGVza3RvcC1wYXNzLWlhbi10b21taW5z",
                result.Value.Value.BoardingPasses.Single(boardingPass => boardingPass.PassengerCustomerId == "9962652476066").DesktopBoardingPassBase64);
            Assert.Equal(
                "ZGVza3RvcC1wYXNzLWFuZHJldy1vdHRleQ==",
                result.Value.Value.BoardingPasses.Single(boardingPass => boardingPass.PassengerCustomerId == "3962652476058").DesktopBoardingPassBase64);
        }

        [Fact]
        public async Task GetBoardingPassByPNRAsync_ShouldNotReturnQRResposne_ForNoneCheckedInPsgsInPnr()
        {
            var result = await _checkInService.RetrieveBoardingPass(new BoardingPassRequest
            {
                OriginCode = "LHR",
                PnrLocator = "HAND01"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PassengersCheckInStatus.TO_COMPLETE, result.Value.Value.CheckInStatus);
            Assert.Equal(6, result.Value.Value.BoardingPasses.Count());
            Assert.False(result.Value.Value.BoardingPasses.Any(x => x.CheckedIn));
            Assert.True(result.Value.Value.BoardingPasses.All(x => !x.CheckedIn));
            Assert.Null(result.Value.Value.BoardingPasses.FirstOrDefault(x => !x.CheckedIn).QrCode);
        }

        [Fact]
        public async Task GetBoardingPassByPNRAsync_ShouldReturnQRResposne_ForCheckedInPsgsInPnr()
        {
            var result = await _checkInService.RetrieveBoardingPass(new BoardingPassRequest
            {
                OriginCode = "DTW",
                PnrLocator = "F747BD"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PassengersCheckInStatus.COMPLETED, result.Value.Value.CheckInStatus);
            Assert.Equal(3, result.Value.Value.BoardingPasses.Count());

            Assert.Equal(new LocalTime(4, 05), result.Value.Value.BoardingPasses.First().BoardingTimeLocalTime);
            Assert.Equal(new LocalTime(14, 05), result.Value.Value.BoardingPasses.Skip(1).First().BoardingTimeLocalTime);
            Assert.Null(result.Value.Value.BoardingPasses.Skip(2).First().BoardingTimeLocalTime);

            Assert.True(result.Value.Value.BoardingPasses.All(x => x.CheckedIn));
            Assert.False(result.Value.Value.BoardingPasses.Any(x => !x.CheckedIn));
            Assert.True(result.Value.Value.BoardingPasses.All(x => x.QrCode != null));
        }

        [Fact]
        public async Task GetBoardingPassByPNRAsync_SthouldThorwError_ForPnrNotFound()
        {
            var request = new BoardingPassRequest
            {
                OriginCode = "LHR",
                PnrLocator = "LHR404"
            };

            await Assert.ThrowsAsync<HttpRequestException>(async () => await _checkInService.RetrieveBoardingPass(request));
        }

        [Fact]
        public async Task GetBoardingPassByPNRAsync_StatusShouldReturnResponse_ForPnrAdvisory()
        {
            var result = await _checkInService.RetrieveBoardingPass(new BoardingPassRequest
            {
                OriginCode = "LHR",
                PnrLocator = "EZ3UPQ"
            });

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(PassengersCheckInStatus.COMPLETED, result.Value.Value.CheckInStatus);
            Assert.Equal(1, result.Value.Value.BoardingPasses.Count());
            Assert.True(result.Value.Value.BoardingPasses.All(x => x.CheckedIn));
            Assert.False(result.Value.Value.BoardingPasses.Any(x => !x.CheckedIn));
            Assert.True(result.Value.Value.BoardingPasses.All(x => x.QrCode != null));
        }

        [Fact]
        public async Task SelectPassenger_ShouldReturn_ProhibitedSSR_Advisory_OnSelect()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND11",
                OriginCode = "LHR"
            })).Value.Value;

            var result = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                PnrLocator = checkInResponse.Reference.PnrLocator,
                FirstName = checkInResponse.Passengers.FirstOrDefault().GivenNames,
                LastName = checkInResponse.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
        {
            checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId
        }
            })).Value.Value;

            Assert.False(result.OnlineCheckInStatus.Available);
            Assert.Equal("PROHIBITEDSSR", result.OnlineCheckInStatus.Code);
            Assert.Equal("Airport check in required", result.OnlineCheckInStatus.Title);
            Assert.StartsWith("Unfortunately", result.OnlineCheckInStatus.Message);
        }

        [Fact]
        public async Task CheckInResponse_ShouldReturn_Advisory_forUMNR_Pnrs()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "UMNRPR",
                OriginCode = "LHR"
            })).Value.Value;

            Assert.IsType<OnlineCheckInStatus>(checkInResponse.OnlineCheckInStatus);
            Assert.False(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal("UMNR", checkInResponse.OnlineCheckInStatus.Code);
            Assert.Equal("Airport check in required", checkInResponse.OnlineCheckInStatus.Title);
            Assert.StartsWith("Young travellers can’t check in", checkInResponse.OnlineCheckInStatus.Message);
            Assert.Equal("We can't check you in online if you wish to enroll in the Delta Unaccompanied Minor program. This requires you to check in at the airport with an agent.", checkInResponse.OnlineCheckInStatus.AdvisoryText1);
        }

        [Fact]
        public async Task InitiateCheckIn_ShouldMapAdvisoryText1ToOnlineCheckInStatus_WhenCheckInIsTooLate()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "EMYQZW",
                OriginCode = "LHR",
                GivenNames = "ALEX",
                Surname = "COTTER"
            })).Value.Value;

            Assert.False(checkInResponse.OnlineCheckInStatus.Available);
            Assert.Equal("checkinTooLateTripInternational", checkInResponse.OnlineCheckInStatus.Code);
            Assert.Equal("We're sorry, but it's too late to check you in for your {0} flight to {1}. View your reservation details at {2}.", checkInResponse.OnlineCheckInStatus.AdvisoryText1);
        }

        [Fact]
        public async Task SelectPassenger_ShouldReturn_PassengerSelectionError_WhenSomePassengersAreNotSelectedBySsci()
        {
            var checkInResponse = (await _checkInService.InitiateCheckInAsync(new Domain.CheckInRequest
            {
                PnrLocator = "HAND02",
                OriginCode = "LHR"
            })).Value.Value;

            var result = (await _checkInService.SelectPassengersAsync(new SelectPassengerRequest
            {
                TravelPartyId = checkInResponse.TravelPartyId,
                PnrLocator = checkInResponse.Reference.PnrLocator,
                FirstName = checkInResponse.Passengers.FirstOrDefault().GivenNames,
                LastName = checkInResponse.Passengers.FirstOrDefault().Surname,
                SelectedPassengerIds = new List<string>
                {
                    checkInResponse.Passengers.FirstOrDefault(x => x.GivenNames.Is("IAN")).PassengerId,
                    checkInResponse.Passengers.Skip(3).FirstOrDefault().PassengerId,
                    checkInResponse.Passengers.Skip(2).FirstOrDefault().PassengerId
                }
            })).Value.Value;

            Assert.False(result.OnlineCheckInStatus.Available);
            Assert.Equal("PassengerSelectionError", result.OnlineCheckInStatus.Code);
            Assert.False(result.OnlineCheckInStatus.Title.IsNullOrEmpty());
            Assert.Equal("We can't complete check in online", result.OnlineCheckInStatus.Title);
            Assert.False(result.OnlineCheckInStatus.Message.IsNullOrEmpty());
            Assert.StartsWith("Looks like something needs sorting at the airport", result.OnlineCheckInStatus.Message);
        }
    }
}
