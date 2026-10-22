using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public sealed class HolidayQueryTests
    {
        private readonly IServiceProvider _serviceProvider;

        public HolidayQueryTests()
        {
            Setup.CreateTestEnvironmentSettings();

            ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:Url", "https://testresources.virginatlantic.com/dev/digital/holidays");
            ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:OcpSubID", Guid.NewGuid().ToString());
            ConfigReader.Set(Constants.Environment.HertzManageBookingUrl, "https://testurl");

            // We need to use Moq as this code is not TDD/DI safe 

            var identityMsResponseJson = @"{
                ""sub"": ""1234567890"",
                ""name"": ""John Doe"",
                ""email"": ""johndoe@example.com"",
                ""roles"": [""User""],
                ""azp"": ""my-client-id"",
                ""iss"": ""https://my-tenant-name.b2clogin.com/my-tenant-name.onmicrosoft.com/v2.0/"",
                ""fcnumber"": ""1234567890""
            }";

            var tokenB2CProviderMoq = new Mock<ITokenB2CProvider<string>>();

            tokenB2CProviderMoq.Setup(m => m.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(identityMsResponseJson);

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                                                      .AddSingleton<ITripsDetailsQueryProvider, TripsDetailsQueryProvider>()
                                                      .AddSingleton<ITokenB2CProvider<string>>(tokenB2CProviderMoq.Object)
                                                      .AddSingleton<IOrderDataProvider, OrderDataProvider>()
                                                      .AddSingleton<IHolidayProvider, HolidayProvider>()
                                                      .AddSingleton<IOperationalScheduleProvider, OperationalScheduleProvider>()
                                                      .AddSingleton<IStarlinkEligibilityProvider, StarlinkEligibilityProvider>()
                                                      .AddSingleton<ITripAircraftEligibilityService, TripAircraftEligibilityService>()
                                                      .AddSingleton<TripDetailsListQuery>()
                                                      .AddSingleton<HolidayQuery>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void RetrieveHolidayBookingDocument_DoesNotHaveAuthorizeAttribute()
        {
            var method = typeof(HolidayQuery).GetMethod(nameof(HolidayQuery.RetrieveHolidayBookingDocument));

            Assert.NotNull(method);
            Assert.Null(method.GetCustomAttributes(typeof(AuthorizeAttribute), false).SingleOrDefault());
        }

        [Fact]
        public async Task RetrieveHolidayBookingDocument_InvalidDocumentToken_ReturnsControlledBadRequest()
        {
            var holidayQuery = _serviceProvider.GetRequiredService<HolidayQuery>();
            var request = new HolidayDocumentDownloadRequest
            {
                DocumentId = "51237351",
                DocumentToken = "invalid-document-token"
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
                holidayQuery.RetrieveHolidayBookingDocument(request, default));

            var error = Assert.Single(exception.Errors);
            Assert.Equal("400", error.Code);
            Assert.Equal("The document token is invalid or expired.", error.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBookingDocument_MismatchedDocumentId_ReturnsControlledBadRequest()
        {
            var holidayQuery = _serviceProvider.GetRequiredService<HolidayQuery>();
            var documentToken = await HolidayDocumentTokenHelper.CreateAsync(
                "booking-123",
                "document-123",
                "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/TRVDOC/document-123.pdf");
            var request = new HolidayDocumentDownloadRequest
            {
                DocumentId = "different-document",
                DocumentToken = documentToken
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
                holidayQuery.RetrieveHolidayBookingDocument(request, default));

            var error = Assert.Single(exception.Errors);
            Assert.Equal("400", error.Code);
            Assert.Equal("The requested document does not match the document token.", error.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBookingDocument_DocumentNotFound_ReturnsControlledNotFound()
        {
            var holidayQuery = _serviceProvider.GetRequiredService<HolidayQuery>();
            var documentToken = await HolidayDocumentTokenHelper.CreateAsync(
                "booking-404",
                "document-404",
                "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-404/TRVDOC/document-404.pdf");
            var request = new HolidayDocumentDownloadRequest
            {
                DocumentId = "document-404",
                DocumentToken = documentToken
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
                holidayQuery.RetrieveHolidayBookingDocument(request, default));

            var error = Assert.Single(exception.Errors);
            Assert.Equal("404", error.Code);
            Assert.Equal("The holiday document could not be found.", error.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_NullRequest_ThrowsException()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;

            await Assert.ThrowsAsync<ArgumentNullException>(() => holidayQuery.RetrieveHolidayBooking(null));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_NullBookingReference_ThrowsException()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = null,
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            // pass the request with a null BookingReference to trigger the exception
            await Assert.ThrowsAsync<ArgumentNullException>(() => holidayQuery.RetrieveHolidayBooking(request));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ErrorFromMS_ThrowsException()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "3232333",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => holidayQuery.RetrieveHolidayBooking(request));
            Assert.Contains(Constants.ErrorCodes.RetrieveHolidayBooking, exception.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithResponseFromAir4_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "1234567890",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "KOPACZ",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(3, booking.Product!.Count());
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);
            Assert.DoesNotContain(booking.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));

            var air4Trip = booking.Product!.FirstOrDefault(x => x.Reference?.PnrLocator.Is("FD2D95") == true);

            Assert.NotNull(air4Trip);
            Assert.True(air4Trip.SupportedFlight);
            Assert.Single(air4Trip!.Slices!);
            Assert.Equal("MBJ", air4Trip.Slices!.First().Destination!.Code);
            Assert.NotNull(air4Trip?.Reference?.PnrLocator);

        }

        [Fact]
        public void HolidaySegment_VSFlight_ReturnsPopsTimings()
        {
            var source = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
            {
                Id = "VS-SEGMENT",
                OperatingCarrier = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.CarrierDetails
                {
                    CarrierCode = "VS",
                    CarrierName = "Virgin Atlantic"
                },
                ScheduledDepartureUtcDateTime = new DateTime(2025, 12, 13, 8, 55, 0)
            };

            var segment = (VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.Segment)(
                source,
                Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData.AirportRefData>());

            Assert.Equal(new DateTime(2025, 12, 6, 8, 55, 0), segment.PopsOpenDateTimeUtc);
            Assert.Equal(new DateTime(2025, 12, 12, 8, 55, 0), segment.PopsCloseDateTimeUtc);
        }

        [Fact]
        public void HolidaySegment_KoreanAirFlight_ReturnsHelpText()
        {
            var source = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
            {
                Id = "KE-SEGMENT",
                OperatingCarrier = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.CarrierDetails
                {
                    CarrierCode = "KE",
                    CarrierName = "Korean Air"
                }
            };

            var segment = (VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.Segment)(
                source,
                Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData.AirportRefData>());

            Assert.Equal("This flight is with Korean Air", segment.OtherAirlineOperatedHelpTitle);
            Assert.Equal(ConfigReader.Get(Constants.Environment.KoreanAirlineOperatedHelpText), segment.OtherAirlineOperatedHelpText);
            Assert.Null(segment.PopsOpenDateTimeUtc);
            Assert.Null(segment.PopsCloseDateTimeUtc);
        }

        [Fact]
        public void HolidaySegment_TouchdownLegs_PreserveEachLegOriginAndDestination()
        {
            var source = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
            {
                Id = "VS185",
                Legs =
                [
                    new()
                    {
                        Origin = new() { Code = "LHR" },
                        Destination = new() { Code = "BGI" }
                    },
                    new()
                    {
                        Origin = new() { Code = "BGI" },
                        Destination = new() { Code = "ANU" }
                    }
                ]
            };

            var segment = (VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.Segment)(
                source,
                Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData.AirportRefData>());

            Assert.Collection(
                segment.Legs!,
                leg =>
                {
                    Assert.Equal("LHR", leg.Origin?.Code);
                    Assert.Equal("BGI", leg.Destination?.Code);
                },
                leg =>
                {
                    Assert.Equal("BGI", leg.Origin?.Code);
                    Assert.Equal("ANU", leg.Destination?.Code);
                });
        }

        [Fact]
        public async Task RetrieveHolidayBooking_MultiLegSupportedFlight_MapsEveryLeg()
        {
            var holidayProvider = _serviceProvider.GetRequiredService<IHolidayProvider>();
            var response = await holidayProvider.RetrieveBooking(new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest
            {
                BookingReference = "7007912",
                DateOfBirth = DateOnly.Parse("1980-01-01"),
                Surname = "User",
                Using = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.RequestOptionCodeType.HOLIDAYS
            });

            var sourceFlightProduct = Assert.Single(response.Booking.HolidayProducts.Where(product => product.TbxProductType == "FLT"));
            var product = (Product)(sourceFlightProduct, Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData.AirportRefData>());

            Assert.Collection(
                product.Slices!.SelectMany(slice => slice.Segments!).SelectMany(segment => segment.Legs!),
                leg => AssertLegRoute(leg, "LHR", "BGI"),
                leg => AssertLegRoute(leg, "BGI", "GND"),
                leg => AssertLegRoute(leg, "GND", "BGI"),
                leg => AssertLegRoute(leg, "BGI", "LHR"));
        }

        private static void AssertLegRoute(Leg leg, string origin, string destination)
        {
            Assert.Equal(origin, leg.Origin?.Code);
            Assert.Equal(destination, leg.Destination?.Code);
            Assert.Null(leg.ScheduledDepartureLocal);
            Assert.Null(leg.ScheduledArrivalLocal);
        }

        [Fact]
        public void HolidayAirportDetails_WithoutReferenceData_PreservesSourceValues()
        {
            var source = new VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.AirportDetails
            {
                Code = "ZZZ",
                AirportName = "Source Airport",
                CityName = "Source City",
                CountryCode = "ZZ",
                CountryName = "Source Country",
                TerminalName = "T1"
            };

            var airport = (VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.AirportDetails)(
                source,
                Enumerable.Empty<VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData.AirportRefData>());

            Assert.Equal("Source Airport", airport.AirportName);
            Assert.Equal("Source City", airport.CityName);
            Assert.Equal("ZZ", airport.CountryCode);
            Assert.Equal("Source Country", airport.CountryName);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ShouldPopulateAirlineName_FromReferenceData_ForUnsupportedHolidayFlights()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "9999998",
                DateOfBirth = DateOnly.Parse("1970-01-01"),
                Surname = "User",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);

            var airlineProduct = booking.Product!.First(p => p.BookingType == "AIRLINE");
            Assert.False(airlineProduct.SupportedFlight);
            Assert.NotNull(airlineProduct.Slices);

            var slice = airlineProduct.Slices!.First();
            Assert.NotNull(slice.Segments);
            Assert.Equal("Dubai", slice.Destination?.CityName);

            var segment = slice.Segments!.First();
            Assert.Equal("https://www.emirates.com/", segment.ManageFlightLink);
            Assert.NotNull(segment.OperatingCarrier);
            Assert.NotNull(segment.MarketingCarrier);

            Assert.Equal("EK", segment.OperatingCarrier!.CarrierCode);
            Assert.Equal("Emirates Airline", segment.OperatingCarrier.CarrierName);
            Assert.Equal("EK", segment.MarketingCarrier!.CarrierCode);
            Assert.Equal("Emirates Airline", segment.MarketingCarrier.CarrierName);
            Assert.Equal("This flight is with Emirates Airline", segment.OtherAirlineOperatedHelpTitle);
            Assert.Equal(ConfigReader.Get(Constants.Environment.OtherAirlineOperatedHelpText), segment.OtherAirlineOperatedHelpText);
            Assert.Null(segment.PopsOpenDateTimeUtc);
            Assert.Null(segment.PopsCloseDateTimeUtc);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_HotelSummaryContent_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "1234567890",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "KOPACZ",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.HolidayBookingSummary);
            Assert.NotNull(booking.HolidayBookingSummary.BrandName);
            Assert.NotNull(booking.HolidayBookingSummary.BrandCode);
        }


        [Fact]
        public async Task RetrieveHolidayBooking_FlightProductMissingLeadPax_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "7003024",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "test",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            //assert that where there is flight product tbxproductid is FLT~7 then the lead pax is missing in the travellers list
            var flightProduct = booking.Product!.FirstOrDefault(x => x.BookingType == "AIRLINE" && x.TbxProductId == "FLT~7");
            Assert.DoesNotContain(flightProduct?.Passengers, x => x.LeadPassenger == true);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithMultipleCars_ShouldMapCarProductIdToPickupAndDropoffDetails()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest
            {
                BookingReference = "6833127",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "boalch",
                Using = RequestOptionCodeType.HOLIDAYS
            };
            var booking = await holidayQuery.RetrieveHolidayBooking(request);
            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);
            var carProducts = booking.Product.Where(product => product.TbxProductType == "CAR" && product.CarDetails != null).ToList();
            Assert.Equal(6, carProducts.Count);
            var pickupProducts = carProducts.Where(product => product.CarDetails!.StartDateLocal != null).ToList();
            var dropOffProducts = carProducts.Where(product => product.CarDetails!.EndDateLocal != null).ToList();
            Assert.Equal(3, pickupProducts.Count);
            Assert.Equal(3, dropOffProducts.Count);
            var expectedCarProductIds = new[]
            {
               "CAR~1",
               "CAR~2",
               "CAR~3"
            };
            var actualCarProductIds = carProducts.Select(product => product.TbxProductId).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(id => id).ToArray();
            Assert.Equal(expectedCarProductIds, actualCarProductIds);
            Assert.All(carProducts, product =>
            {
                Assert.False(string.IsNullOrWhiteSpace(product.TbxProductId));
                Assert.False(
                    string.IsNullOrWhiteSpace(product.CarDetails!.CarProductId));
                Assert.Equal(product.TbxProductId, product.CarDetails.CarProductId, ignoreCase: true);
            });

            foreach (var carProductId in expectedCarProductIds)
            {
                var pickupForCar = pickupProducts
                    .Where(product => product.CarDetails!.CarProductId!.Equals(carProductId, StringComparison.OrdinalIgnoreCase)).ToList();
                var dropOffForCar = dropOffProducts
                    .Where(product => product.CarDetails!.CarProductId!.Equals(carProductId, StringComparison.OrdinalIgnoreCase)).ToList();
                Assert.Single(pickupForCar);
                Assert.Single(dropOffForCar);
                Assert.Null(pickupForCar[0].CarDetails!.EndDateLocal);
                Assert.Null(pickupForCar[0].CarDetails!.SelectedReturnLocation);
                Assert.Null(dropOffForCar[0].CarDetails!.StartDateLocal);
                Assert.Null(dropOffForCar[0].CarDetails!.SelectedPickupLocation);
            }
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithResponse_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997721",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Burgess",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(7, booking.Product!.Count());
            Assert.DoesNotContain(booking.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            var air4Trip = booking.Product!.FirstOrDefault(x => x.Reference?.PnrLocator.Is("C4F2RI") == true);

            Assert.NotNull(air4Trip);
            Assert.False(air4Trip.SupportedFlight);
            Assert.Single(air4Trip!.Slices!);
            Assert.Equal("BGI", air4Trip.Slices!.First().Destination!.Code);
            Assert.NotNull(air4Trip?.SortByDate);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithTravellers_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6609014",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Simpson",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);
            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(3, booking.Product!.Count());
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);
            Assert.DoesNotContain(booking.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));

            var air4Trip = booking.Product!.FirstOrDefault(x => x.Reference?.PnrLocator.Is("DZ76P3") == true);

            Assert.NotNull(air4Trip);
            Assert.True(air4Trip.SupportedFlight);
            Assert.NotNull(booking?.Payment);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithPassengers_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(5, booking.Product!.Count());
            Assert.DoesNotContain(booking.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            // Assert all products have booking type value
            Assert.All(booking.Product, product => Assert.False(string.IsNullOrEmpty(product.BookingType)));

            var air4Trip = booking.Product!.FirstOrDefault(x => x.SupportedFlight && x.Reference?.PnrLocator.Is("ETNWM4") == true);

            Assert.NotNull(air4Trip);
            Assert.True(air4Trip.SupportedFlight);
            Assert.Single(air4Trip!.Slices!);
            Assert.NotNull(air4Trip.Passengers);
            Assert.NotNull(booking?.Payment);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithMultiplePNR_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997310",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(12, booking.Product!.Count());
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            // Assert all products have booking type value
            Assert.All(booking.Product, product => Assert.False(string.IsNullOrEmpty(product.BookingType)));

            var air4Trip = booking.Product!.FirstOrDefault(x => x.SupportedFlight && x.Reference?.PnrLocator.Is("DD4XOH") == true);

            Assert.NotNull(air4Trip);
            Assert.True(air4Trip.SupportedFlight);
            Assert.Single(air4Trip!.Slices!);
            Assert.NotNull(air4Trip.Passengers);
            Assert.NotNull(booking?.Payment);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithTransfer_ReturnPickUpDropOffPoints()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "7003539",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.Equal(5, booking.Product?.Count());

            var transferBookingData = booking.Product?.Where(x => x.BookingType == "TRANSFER");
            Assert.NotNull(transferBookingData);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.SelectedDropoffPoint?.CityCode != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.SelectedPickupPoint?.CityCode != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.DropoffPointLocation != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.PickupPointLocation != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.Highlight != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.StartDate != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.ElementGroup != null);
            Assert.Contains(transferBookingData, x => x.ExtraDetails?.ElementGroupName != null);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithTransfer_VerifyLocation()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "7003539",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.Equal(5, booking.Product?.Count());

            var transferBookingData = booking.Product?.Where(x => x.BookingType == "TRANSFER");
            Assert.NotNull(transferBookingData);
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedDropoffPoint?.CityName));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedDropoffPoint?.CountryName));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedPickupPoint?.CountryName));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedPickupPoint?.CityName));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedPickupPoint?.LocationAddress));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.SelectedDropoffPoint?.LocationAddress));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.PickupPointLocation));
            Assert.All(transferBookingData, x => Assert.NotNull(x.ExtraDetails?.DropoffPointLocation));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithHotel_VerifyLocation()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6834316",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Dowling",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);

            var hotels = booking.Product?.Where(x => x.BookingType == "HOTEL");
            Assert.NotNull(hotels);
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.CityName));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.CountryName));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.LocationAddress));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithHotel_VerifyHotelDetailsInformation()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6834316",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Dowling",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);

            var hotels = booking.Product?.Where(x => x.BookingType == "HOTEL");
            Assert.NotNull(hotels);
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.CityName));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.CountryName));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails?.Destination?.LocationAddress));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails.NumberOfRooms));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails.TravelDuration.Duration));
            Assert.All(hotels, x => Assert.NotNull(x.HotelDetails.TravelDuration.DurationType));
            Assert.All(hotels, hotel => Assert.True(hotel.HotelDetails.Rooms.All(room => room.PartyComposition != null)));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithCar_VerifyLocation()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997310",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);

            var carPickup = booking.Product?.Where(x => x.BookingType == "CAR" && x.CarDetails?.StartDateLocal != null);
            var carDropOff = booking.Product?.Where(x => x.BookingType == "CAR" && x.CarDetails?.EndDateLocal != null);
            Assert.NotNull(carPickup);
            Assert.All(carPickup, x => Assert.NotNull(x.CarDetails?.SelectedPickupLocation?.CityName));
            Assert.All(carDropOff, x => Assert.NotNull(x.CarDetails?.SelectedReturnLocation?.CountryName));
            Assert.All(carPickup, x => Assert.NotNull(x.CarDetails?.SelectedPickupLocation?.CountryName));
            Assert.All(carDropOff, x => Assert.NotNull(x.CarDetails?.SelectedReturnLocation?.CityName));
            Assert.All(carDropOff, x => Assert.NotNull(x.CarDetails?.SelectedReturnLocation?.LocationAddress));
            Assert.All(carPickup, x => Assert.NotNull(x.CarDetails?.SelectedPickupLocation?.LocationAddress));
        }

        [Theory]
        [InlineData("6833025")]
        [InlineData("6833026")]
        [InlineData("6833127")]
        public async Task RetrieveHolidayBooking_CarFixturesRemainInFuture(string bookingReference)
        {
            var earliestMockDate = DateTime.Today;
            var query = _serviceProvider.GetRequiredService<HolidayQuery>();
            var booking = await query.RetrieveHolidayBooking(new TripsDetailsRequest
            {
                BookingReference = bookingReference,
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "boalch",
                Using = RequestOptionCodeType.HOLIDAYS
            });
            var latestMockDate = DateTime.Today;

            Assert.NotNull(booking);
            Assert.NotNull(booking.TravelDates?.DepartureDate);
            Assert.NotNull(booking.TravelDates.ReturnDate);
            Assert.InRange(booking.TravelDates.DepartureDate.Value.Date,
                LocalDate.FromDateTime(earliestMockDate.AddDays(30)),
                LocalDate.FromDateTime(latestMockDate.AddDays(30)));
            Assert.InRange(booking.TravelDates.ReturnDate.Value.Date,
                LocalDate.FromDateTime(earliestMockDate.AddDays(47)),
                LocalDate.FromDateTime(latestMockDate.AddDays(47)));
            Assert.False(BookingIdHelper.IsHolidayBookingExpired(booking));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_Verify_CarDetails()
        {
            // The mock commands use local time. Bracket evaluation to allow a midnight rollover.
            var earliestMockDate = DateTime.Today;
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6833025",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "boalch",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            var latestMockDate = DateTime.Today;

            Assert.NotNull(booking);

            Assert.NotNull(booking.Product);

            var carProducts = booking.Product.Where(p => p.BookingType == "CAR").ToList();

            Assert.NotEmpty(carProducts);

            var pickup = Assert.Single(carProducts, car => car.CarDetails?.SelectedPickupLocation != null).CarDetails;
            var dropOff = Assert.Single(carProducts, car => car.CarDetails?.SelectedReturnLocation != null).CarDetails;
            Assert.NotNull(pickup);
            Assert.NotNull(dropOff);
            Assert.NotNull(pickup.StartDateLocal);
            Assert.Null(pickup.EndDateLocal);
            Assert.NotNull(dropOff.EndDateLocal);
            Assert.Null(dropOff.StartDateLocal);
            Assert.InRange(pickup.StartDateLocal.Value.Date,
                LocalDate.FromDateTime(earliestMockDate.AddDays(30)),
                LocalDate.FromDateTime(latestMockDate.AddDays(30)));
            Assert.Equal(new LocalTime(16, 25), pickup.StartDateLocal.Value.TimeOfDay);
            Assert.InRange(dropOff.EndDateLocal.Value.Date,
                LocalDate.FromDateTime(earliestMockDate.AddDays(46)),
                LocalDate.FromDateTime(latestMockDate.AddDays(46)));
            Assert.Equal(new LocalTime(16, 0), dropOff.EndDateLocal.Value.TimeOfDay);

            Assert.All(carProducts, car =>
            {
                Assert.NotNull(car.CarDetails);

                // Car information assertions
                Assert.False(string.IsNullOrWhiteSpace(car.CarDetails?.Name));
                Assert.Equal("Nissan - Versa or similar - Compact 4 door", car.CarDetails?.Name);
                Assert.Equal("Nissan", car.CarDetails?.Make);
                Assert.Equal("Versa or similar", car.CarDetails?.Model);
                Assert.Equal("CDAR", car.CarDetails?.Class);
                Assert.Equal("Compact 4 door", car.CarDetails?.ClassName);
                Assert.Equal(4, car.CarDetails?.DoorCount);
                Assert.Equal(true, car.CarDetails?.AirConditioned);
                Assert.Equal(" ", car.CarDetails?.SippCode);

                // Product details
                Assert.NotNull(car.CarDetails?.CarProduct);
                Assert.Equal(4130, car.CarDetails?.CarProduct?.Id);
                Assert.False(string.IsNullOrWhiteSpace(car.CarDetails?.CarProduct?.Name));
                Assert.Equal("Hertz Go ", car.CarDetails?.CarProduct?.Name);

                // Supplier information
                Assert.NotNull(car.CarDetails?.SupplierInformation);
                Assert.False(string.IsNullOrWhiteSpace(car.CarDetails?.SupplierInformation?.Name));
                Assert.Equal("Hertz", car.CarDetails?.SupplierInformation?.Name);
                Assert.False(string.IsNullOrWhiteSpace(car.CarDetails?.SupplierInformation?.Code));
                Assert.Equal("HERTZ", car.CarDetails?.SupplierInformation?.Code);
                Assert.Equal("L1264130821", car.CarDetails?.SupplierInformation?.ConfirmationNo);
                Assert.Equal("https://testurl", car.CarDetails?.ManageBookingUrl);

                // Traveller composition
                Assert.NotNull(car.CarDetails?.TravellersComposition);
                Assert.Equal(2, car.CarDetails?.TravellersComposition?.AdultsCount);
                Assert.Equal(0, car.CarDetails?.TravellersComposition?.ChildrenCount);
                Assert.Equal(0, car.CarDetails?.TravellersComposition?.InfantCount);

                // Locations
                if (car.CarDetails?.SelectedPickupLocation != null)
                {
                    Assert.Equal("Hertz\n\n*Car pick up/drop off is A side*\n\nOrlando International Airport\n1 Airport Blvd\nORLANDO\nFL 32812\n\nOpening times\n 24/7", car.CarDetails.SelectedPickupLocation.LocationAddress);
                }
                if (car.CarDetails?.SelectedReturnLocation != null)
                {
                    Assert.Equal("Hertz\n\n*Car pick up/drop off is A side*\n\nOrlando International Airport\n1 Airport Blvd\nORLANDO\nFL 32812\n\nOpening times\n 24/7", car.CarDetails.SelectedReturnLocation.LocationAddress);
                }

            });

        }
        [Fact]
        public async Task RetrieveHolidayBooking_Verify_CarDetails_NonHertzSupplierFlagFalse()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6833026",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "boalch",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);

            var carProducts = booking.Product.Where(p => p.BookingType == "CAR").ToList();

            Assert.NotEmpty(carProducts);

            Assert.All(carProducts, car =>
            {
                Assert.NotNull(car.CarDetails);
                Assert.Equal("Avis", car.CarDetails.SupplierInformation?.Name);
                Assert.Equal("AVIS", car.CarDetails.SupplierInformation?.Code);
                Assert.Null(car.CarDetails.ManageBookingUrl);
            });
        }


        [Fact]
        public async Task RetrieveHolidayBooking_PastFlightPNR_ReturnStatusInLeg()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6999382",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            // Assert all products have booking type value
            Assert.All(booking.Product, product => Assert.False(string.IsNullOrEmpty(product.BookingType)));
            Assert.All(booking.Product.Where(p => p.BookingType == "AIRLINE").SelectMany(s => s.Slices.SelectMany(sl => sl.Segments.SelectMany(seg => seg.Legs))), leg => Assert.False(string.IsNullOrEmpty(leg.Status)));
            Assert.All(booking.Product.Where(p => p.BookingType == "AIRLINE").SelectMany(s => s.Slices.SelectMany(sl => sl.Segments.SelectMany(seg => seg.Legs))), leg => Assert.Equal("FLIGHT_COMPLETE", leg.Status));

        }

        [Fact]
        public async Task RetrieveHolidayBooking_7DaysPastBookingError()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6999383",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => holidayQuery.RetrieveHolidayBooking(request));
            Assert.Contains(Constants.ErrorCodes.RetrieveHolidayBooking, exception.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithSeparateCarProduct_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997310",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);

            var carProducts = booking.Product.Where(p => p.BookingType == "CAR").ToList();
            Assert.NotEmpty(carProducts);
            Assert.All(carProducts, car => Assert.NotNull(car.CarDetails));
            Assert.Equal(carProducts.Count(car => car?.CarDetails?.StartDateLocal != null), carProducts.Count(car => car?.CarDetails?.EndDateLocal != null));

            //Assert if car product has start date then end date should be null and vice versa
            Assert.All(carProducts, car =>
            {
                if (car?.CarDetails?.StartDateLocal != null)
                {
                    Assert.Null(car.CarDetails.EndDateLocal);
                }
                else if (car?.CarDetails?.EndDateLocal != null)
                {
                    Assert.Null(car.CarDetails.StartDateLocal);
                }
            });
            // Assert if car product have start date then pickup point should not be null and if end date then return point should not be null
            Assert.All(carProducts, car =>
            {
                if (car?.CarDetails?.StartDateLocal != null)
                {
                    Assert.NotNull(car.CarDetails.SelectedPickupLocation);
                }
                else if (car?.CarDetails?.EndDateLocal != null)
                {
                    Assert.NotNull(car.CarDetails.SelectedReturnLocation);
                }
            });
            Assert.All(carProducts, car => Assert.NotEqual(car.CarDetails.StartDateLocal.HasValue, car.CarDetails.EndDateLocal.HasValue));
            Assert.All(carProducts, car =>
            {
                if (car?.CarDetails?.StartDateLocal != null)
                {
                    Assert.Equal(car.CarDetails.StartDateLocal.Value.Date, car.SortByDate.Date);
                }
                else if (car?.CarDetails?.EndDateLocal != null)
                {
                    Assert.Equal(car.CarDetails.EndDateLocal.Value.Date, car.SortByDate.Date);
                }
            });
        }

        [Fact]
        public async Task RetrieveHolidayBooking_DisneyBookings_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996566",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            //assert that whenever there is an EXTRA BookingType then Extra details should always be populated
            Assert.All(booking.Product, p =>
            {
                if (p.BookingType == "EXTRA")
                {
                    Assert.NotNull(p.ExtraDetails);
                }
            });
            Assert.Contains(booking.Product, x => x.TbxProductType == "GEN" && x.ExtraDetails.ElementGroup == "DISNEY");
            Assert.Contains(booking.Product, p => p.TbxProductType == "GEN" && p.ExtraDetails?.ElementGroup == "DISNEY" && p.ExtraDetails?.Highlight == "14-Day Disney Ticket." && p.ExtraDetails.Name == "Disney - Park Tickets-14-Day Disney Magic Ticket");
        }


        [Fact]
        public async Task RetrieveHolidayBooking_HotelDisneyBookings_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6834428",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Test",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            //assert that whenever there is an EXTRA BookingType then Extra details should always be populated
            Assert.All(booking.Product, p =>
            {
                if (p.BookingType == "EXTRA")
                {
                    Assert.NotNull(p.ExtraDetails);
                }
            });
            Assert.Equal(3, booking.Product.Where(x => x.BookingType == "EXTRA" && x.ExtraDetails?.ElementGroup == "DISNEY").Count());
            Assert.Contains(booking.Product, x => x.TbxProductType == "GEN" && x.ExtraDetails.ElementGroup == "DISNEY");
            Assert.Contains(booking.Product, p => p.TbxProductType == "GEN" && p.ExtraDetails?.Name == "DisneyTicket - 14-Day Disney Magic Ticket - Standard Room");
        }

        [Fact]
        public async Task RetrieveHolidayBooking_DisneyBookings_DISNEYFC_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996567",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            //assert that whenever there is an EXTRA BookingType then Extra details should always be populated
            Assert.All(booking.Product, p =>
            {
                if (p.BookingType == "EXTRA")
                {
                    Assert.NotNull(p.ExtraDetails);
                }
            });

            var extra = booking.Product.FirstOrDefault(x => x.BookingType == "EXTRA" && x.ExtraDetails?.ElementGroup == "DISNEYFC");
            Assert.True(extra?.ExtraDetails?.Highlight == "14-Day Disney Ticket." && extra.ExtraDetails.Name == "Disney - Park Tickets-14-Day Disney Magic Ticket");
        }

        [Fact]
        public async Task RetrieveHolidayBooking_UniversalBookings_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "7004639",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            //assert that whenever there is an EXTRA BookingType then Extra details should always be populated
            Assert.All(booking.Product, p =>
            {
                if (p.BookingType == "EXTRA")
                {
                    Assert.NotNull(p.ExtraDetails);
                }
            });
            Assert.Contains(booking.Product, x => x.TbxProductType == "GEN" && x.ExtraDetails.ElementGroup == "USAEXC");
            Assert.Contains(booking.Product, p => p.TbxProductType == "GEN" && p.ExtraDetails?.ElementGroup == "USAEXC" && p.ExtraDetails?.Highlight == "Universal 1 Day, 2nd Day Free Ticket" && p.ExtraDetails.Name == " EXP - USA-04-10-2026");
        }

        [Fact]
        public async Task RetrieveHolidayBooking_VerifyOrdering()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997310",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            //Act
            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            //Assert that products are ordered as required timeline order
            var products = booking.Product.ToList();
            // Verify overall ordering is non-decreasing by SortByDate
            for (int i = 0; i < products.Count - 1; i++)
            {
                var bookingTypeCurrent = products[i]?.BookingType;
                var current = bookingTypeCurrent switch
                {
                    "AIRLINE" => products[i]?.Slices?.First().ScheduledDepartureLocal,
                    "HOTEL" => products[i]?.HotelDetails?.CheckIn,
                    "CAR" => products[i]?.CarDetails?.StartDateLocal ?? products[i]?.CarDetails?.EndDateLocal,
                    "TRANSFER" => products[i]?.ExtraDetails?.StartDate,
                    "EXTRA" => products[i]?.ExtraDetails?.StartDate ?? products[i]?.SortByDate,
                    _ => products[i]?.SortByDate
                };

                var bookingTypeNext = products[i + 1]?.BookingType;
                var next = bookingTypeNext switch
                {
                    "AIRLINE" => products[i + 1]?.Slices?.First().ScheduledDepartureLocal,
                    "HOTEL" => products[i + 1]?.HotelDetails?.CheckIn,
                    "CAR" => products[i + 1]?.CarDetails?.StartDateLocal ?? products[i + 1]?.CarDetails?.EndDateLocal,
                    "TRANSFER" => products[i + 1]?.ExtraDetails?.StartDate,
                    "EXTRA" => products[i + 1]?.ExtraDetails?.StartDate ?? products[i + 1]?.SortByDate,
                    _ => products[i + 1]?.SortByDate
                };

                // Compare the full sort timestamp to verify the same ordering precision as production
                Assert.True(current.HasValue, $"Missing sort date for product at index {i}");
                Assert.True(next.HasValue, $"Missing sort date for product at index {i + 1}");
                Assert.True(current.Value.CompareTo(next.Value) <= 0, $"Products not ordered: index {i} ({current.Value}) > index {i + 1} ({next.Value})");
            }
        }

        [Fact]
        public async Task RetrieveHolidayBooking_QuoteBooking_ReturnsError()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6836033",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => holidayQuery.RetrieveHolidayBooking(request));
            Assert.Contains(Constants.ErrorCodes.RetrieveHolidayBookingValidation, exception.Message);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ParkTicketOrder_ReturnsSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996164",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);

            var products = booking.Product.ToList();

            Assert.Contains(products, x => x.BookingType == "EXTRA" && x.ExtraDetails?.IsDisneyTicket == true);
            var disneyTicketIndex = products.FindIndex(p => p.BookingType == "EXTRA" && p.ExtraDetails?.IsDisneyTicket == true);
            //Assert if orlando park tickets have the correct order after orlando hotel
            var orlandoHotelIndex = products.FindIndex(p => p.BookingType == "HOTEL" && p.HotelDetails?.Destination?.CityCode == "ORL");
            if (orlandoHotelIndex < 0)
            {
                var orlandoFlightIndex = products.FindIndex(p => p.BookingType == "AIRLINE" && p.Slices?.First()?.Destination?.Code == "MCO");
                // If there is no Orlando hotel, we can't assert the order, but we can at least assert that the Disney ticket is present
                Assert.NotEqual(-1, disneyTicketIndex);
                Assert.NotEqual(-1, orlandoFlightIndex);
                Assert.True(orlandoFlightIndex >= 0 && orlandoFlightIndex < disneyTicketIndex, $"Orlando Flight should be before Disney ticket in the product list. Flight index: {orlandoFlightIndex}, Disney ticket index: {disneyTicketIndex}");

            }
            else
            {
                Assert.NotEqual(-1, disneyTicketIndex);
                Assert.NotEqual(-1, orlandoHotelIndex);
                Assert.True(orlandoHotelIndex >= 0 && orlandoHotelIndex < disneyTicketIndex, $"Orlando hotel should be before Disney ticket in the product list. Hotel index: {orlandoHotelIndex}, Disney ticket index: {disneyTicketIndex}");
            }
        }

        [Fact]
        public async Task RetrieveHolidayBooking_TransferOrder_Correct()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "7003539",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.Equal(5, booking.Product?.Count());
            var transferBookingData = booking.Product?.Where(x => x.BookingType == "TRANSFER");
            Assert.NotEmpty(transferBookingData ?? []);
            var firstTransfer = transferBookingData?.FirstOrDefault();
            var transferIndex = booking?.Product?.ToList().FindIndex(p => p.BookingType == "TRANSFER");
            var hotelIndex = booking?.Product?.ToList().FindIndex(p => p.BookingType == "HOTEL");
            var flightIndex = booking?.Product?.ToList().FindIndex(p => p.BookingType == "AIRLINE");
            Assert.True(transferIndex.HasValue && transferIndex.Value >= 0 && hotelIndex.HasValue && hotelIndex.Value >= 0 && flightIndex.HasValue && flightIndex.Value >= 0, "Missing expected product types for transfer order test");
            Assert.True(transferIndex.Value > flightIndex.Value, $"Transfer should be after flight. Flight index: {flightIndex}, Transfer index: {transferIndex}");
            Assert.True(transferIndex.Value < hotelIndex.Value, $"Transfer should be before hotel. Hotel index: {hotelIndex}, Transfer index: {transferIndex}");
        }

        [Fact]
        public async Task RetrieveHolidayBooking_MultiTransferOrder_AllTransfersCorrect()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997658",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);

            var products = booking.Product!.ToList();

            var transferIndices = products
                .Select((p, i) => new { Product = p, Index = i })
                .Where(x => x.Product?.BookingType == "TRANSFER")
                .Select(x => x.Index)
                .ToList();

            Assert.True(transferIndices.Count > 1, $"Expected multiple transfer products but found {transferIndices.Count}");

            foreach (var transferIndex in transferIndices)
            {
                Assert.True(products.ElementAt(transferIndex - 1).BookingType == "HOTEL" || products.ElementAt(transferIndex - 1).BookingType == "AIRLINE", $"Transfer at index {transferIndex} should be between HOTEL and AIRLINE products.");
                Assert.True(products.ElementAt(transferIndex + 1).BookingType == "HOTEL" || products.ElementAt(transferIndex + 1).BookingType == "AIRLINE", $"Transfer at index {transferIndex} should be between HOTEL and AIRLINE products.");
                Assert.False(products.ElementAt(transferIndex - 1).BookingType == "HOTEL" && products.ElementAt(transferIndex + 1).BookingType == "HOTEL", $"Transfer at index {transferIndex} should not be between HOTEL and HOTEL products.");
                Assert.False(products.ElementAt(transferIndex - 1).BookingType == "AIRLINE" && products.ElementAt(transferIndex + 1).BookingType == "AIRLINE", $"Transfer at index {transferIndex} should not be between AIRLINE and AIRLINE products.");
            }
        }

        [Fact]
        public async Task RetrieveHolidayDocumentDetails_ReturnsAllDocumentResults()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;

            // Arrange
            var tripResourceId = await BookingIdHelper.CreateBookingId(existingBookingId: null, msBookingId: "6666666", givenName: null, surName: "Test", dob: "1999-09-09", basketId: Guid.NewGuid().ToString());

            var request = new HolidayDocumentDetailsRequest()
            {
                TripResourceId = tripResourceId
            };

            var result = await holidayQuery.RetrieveHolidayBookingDocumentDetails(request, default);

            // Top level
            Assert.NotNull(result);
            Assert.NotNull(result.DocumentInformation);
            Assert.Equal(3, result.DocumentInformation.Count());

            Assert.All(result.DocumentInformation, d => Assert.Contains(d.DocumentType, Constants.Environment.AllowedDocumentTypes));

            // All items belong to the same booking reference
            Assert.All(result.DocumentInformation, doc =>
                Assert.Equal("6666666", doc.BookingReference));

            var documentDetails = result.DocumentInformation.Where(d => !d.DocumentId.IsNullOrWhiteSpace()).ToDictionary(d => d.DocumentId);
            var documentTicketDetails = result.DocumentInformation.FirstOrDefault(d => d.DocumentId.IsNullOrWhiteSpace() && d.DocumentType == "TRVDOC");

            Assert.All(result.DocumentInformation, document => Assert.Null(document.DocumentUrl));

            Assert.True(documentDetails.ContainsKey("51237351"));
            Assert.Equal("CBKINV", documentDetails["51237351"].DocumentType);
            var invoiceToken = await HolidayDocumentTokenHelper.ValidateAsync(documentDetails["51237351"].DocumentToken!);
            Assert.Equal(tripResourceId, invoiceToken.BookingId);
            Assert.Equal("51237351", invoiceToken.DocumentId);
            Assert.Equal("https://vsolpublic.virginholidays.co.uk/document-access-api/document/6666666/CBKINV/51237351.pdf", invoiceToken.DocumentUrl);

            Assert.True(documentDetails.ContainsKey("51237352"));
            Assert.Equal("ATOLCERTIFICATE", documentDetails["51237352"].DocumentType);
            var atolToken = await HolidayDocumentTokenHelper.ValidateAsync(documentDetails["51237352"].DocumentToken!);
            Assert.Equal("51237352", atolToken.DocumentId);
            Assert.Equal("https://vsolpublic.virginholidays.co.uk/document-access-api/document/6666666/ATOLCERTIFICATE/51237352.pdf", atolToken.DocumentUrl);

            Assert.Equal("TRVDOC", documentTicketDetails?.DocumentType);
            Assert.Equal("Tickets", documentTicketDetails?.Name);
            Assert.Equal(404, documentTicketDetails?.Status);
            Assert.Null(documentTicketDetails?.DocumentId);
            Assert.Null(documentTicketDetails?.DocumentToken);

            Assert.False(documentDetails.ContainsKey("51238174"));
        }

        [Fact]
        public async Task RetrieveHolidayDocumentDetails_ReturnsTicketsDocumentAlways()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;

            // Arrange
            var tripResourceId = await BookingIdHelper.CreateBookingId(existingBookingId: null, msBookingId: "6997658", givenName: null, surName: "Test", dob: "1999-09-09", basketId: Guid.NewGuid().ToString());

            var request = new HolidayDocumentDetailsRequest()
            {
                TripResourceId = tripResourceId
            };

            var result = await holidayQuery.RetrieveHolidayBookingDocumentDetails(request, default);

            // Top level
            Assert.NotNull(result);
            Assert.NotNull(result.DocumentInformation);
            Assert.Equal(3, result.DocumentInformation.Count());

            Assert.All(result.DocumentInformation, d => Assert.Contains(d.DocumentType, Constants.Environment.AllowedDocumentTypes));

            // All items belong to the same booking reference
            Assert.All(result.DocumentInformation, doc =>
                Assert.Equal("6997658", doc.BookingReference));

            // Add an assertion that the document type TRVDOC is always present in the result.
            Assert.Contains(result.DocumentInformation, d => d.DocumentType == "TRVDOC" && d.Name == "Tickets" && d.DocumentId == null && d.BookingReference == "6997658" && d.Status == 404);
        }

        [Fact]
        public async Task ValidateRetrieveHolidayDocumentDetails_ReturnsTicketsDocumentAlways()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;

            // Arrange
            var tripResourceId = await BookingIdHelper.CreateBookingId(existingBookingId: null, msBookingId: "6997658", givenName: null, surName: "Test", dob: "1999-09-09", basketId: Guid.NewGuid().ToString());

            var request = new HolidayDocumentDetailsRequest()
            {
                TripResourceId = tripResourceId
            };

            var result = await holidayQuery.RetrieveHolidayBookingDocumentDetails(request, default);

            // Top level
            Assert.NotNull(result);
            Assert.NotNull(result.DocumentInformation);
            Assert.Equal(3, result.DocumentInformation.Count());

            Assert.All(result.DocumentInformation, d => Assert.Contains(d.DocumentType, Constants.Environment.AllowedDocumentTypes));

            // All items belong to the same booking reference
            Assert.All(result.DocumentInformation, doc =>
                Assert.Equal("6997658", doc.BookingReference));

            // Add an assertion that the document type TRVDOC is always present in the result.
            Assert.Contains(result.DocumentInformation, d => d.DocumentType == "TRVDOC" && d.Name == "Tickets" && d.DocumentId == null && d.BookingReference == "6997658" && d.Status == 404);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_SuccessHoliday_ReturnsTripresourceId()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(5, booking.Product!.Count());
            Assert.DoesNotContain(booking.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));
            Assert.NotNull(booking.TripResourceId);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_PaymentHistory_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking?.Payment?.PastPayments);
            Assert.NotEmpty(booking.Payment.PastPayments);
            Assert.All(booking.Payment.PastPayments, payment =>
            {
                Assert.NotNull(payment.CurrencyAmount);
                Assert.NotNull(payment.PaymentCurrency);
                Assert.NotNull(payment.RealisedDate);
                Assert.NotNull(payment.Realised);
                Assert.NotNull(payment.ReceiptDate);
                Assert.NotNull(payment.PaymentMethodCode);
                Assert.True(Enum.IsDefined(typeof(PaymentTransactionType), payment.TransactionType));
            });

            // Regression guard: if Holidays MS stops returning `transactionType` (or deserialization fails),
            // the enum will default and we won't see non-pay types in the mapped model.
            Assert.Contains(booking.Payment.PastPayments, payment => payment.TransactionType == PaymentTransactionType.Refund);
            Assert.Contains(booking.Payment.PastPayments, payment => payment.TransactionType == PaymentTransactionType.Correction);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_PaymentHistory_OrderingMostRecentFirst_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;

            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking?.Payment?.PastPayments);
            Assert.NotEmpty(booking.Payment.PastPayments);

            var payments = booking.Payment.PastPayments.ToList();

            Assert.All(payments, payment => Assert.NotNull(payment.RealisedDate));

            for (var i = 0; i < payments.Count - 1; i++)
            {
                Assert.True(
                    payments[i].RealisedDate!.Value >= payments[i + 1].RealisedDate!.Value,
                    $"Payments are not ordered by RealisedDate descending at index {i}.");
            }
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithMultipleSegmentPNR_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6874036",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Farmer",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(12, booking.Product!.Count());
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            // Assert all products have booking type value
            Assert.All(booking.Product, product => Assert.False(string.IsNullOrEmpty(product.BookingType)));

            var air4Trip = booking.Product!.Where(x => x.SupportedFlight && x.Reference?.PnrLocator.Is("DNC6NL") == true);

            Assert.NotNull(air4Trip);
            Assert.NotEmpty(air4Trip);

            Assert.Multiple(() =>
            {
                Assert.All(air4Trip, trip => Assert.True(trip.SupportedFlight));
                Assert.All(air4Trip, trip => Assert.NotNull(trip.Passengers));
                Assert.Single(air4Trip!.First().Slices!);
                Assert.True(air4Trip.Any(trip => trip.Slices?.FirstOrDefault()?.Segments?.Count() > 1));
                Assert.All(air4Trip.SelectMany(t => t?.Slices ?? []), sl => Assert.NotNull(sl.Id));
                Assert.Equal("PT10H45M", air4Trip.First().Slices!.First().Duration);
            });

            // Assert there are no duplicate products with slices (by reference + slice details)
            var productsWithSlices = booking.Product.Where(p => p.Slices != null && p.Slices.Any()).ToList();
            var sliceProductKeys = productsWithSlices.Select(p =>
                $"{p.BookingType}|{p.Reference?.PnrLocator}|{p.Reference?.OrderId}|" +
                string.Join(";", p.Slices!.Select(s => $"{s?.Origin?.Code}>{s?.Destination?.Code}>{s?.ScheduledDepartureLocal}>{s?.ScheduledArrivalLocal}"))
            );
            Assert.Equal(sliceProductKeys.Count(), sliceProductKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public async Task RetrieveHolidayBooking_WithMultipleSegmentAir4PNR_ReturnSuccess()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6874038",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Farmer",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Product);
            Assert.Equal(11, booking.Product!.Count());
            Assert.NotNull(booking?.Destination);
            Assert.NotNull(booking?.TravelDates);

            // Assert all products have booking type value
            Assert.All(booking.Product, product => Assert.False(string.IsNullOrEmpty(product.BookingType)));

            var air4Trip = booking.Product!.Where(x => x.SupportedFlight && x.Reference?.PnrLocator.Is("DNC6NH") == true);

            Assert.NotNull(air4Trip);
            Assert.NotEmpty(air4Trip);

            Assert.Multiple(() =>
            {
                Assert.All(air4Trip, trip => Assert.True(trip.SupportedFlight));
                Assert.All(air4Trip, trip => Assert.NotNull(trip.Passengers));
                Assert.Single(air4Trip!.First().Slices!);
                Assert.True(air4Trip.Any(trip => trip.Slices?.Any(slice => slice.Segments?.Count() > 1) == true));
            });

            // Assert there are no duplicate products with slices (by reference + slice details)
            var productsWithSlices = booking.Product.Where(p => p.Slices != null && p.Slices.Any()).ToList();
            var sliceProductKeys = productsWithSlices.Select(p =>
                $"{p.BookingType}|{p.Reference?.PnrLocator}|{p.Reference?.OrderId}|" +
                string.Join(";", p.Slices!.Select(s => $"{s?.Origin?.Code}>{s?.Destination?.Code}>{s?.ScheduledDepartureLocal}>{s?.ScheduledArrivalLocal}"))
            );
            Assert.Equal(sliceProductKeys.Count(), sliceProductKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        }

        [Fact]
        public async Task RetrieveHolidayBooking_SuccessHoliday_ReturnsPaymentDetails()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            var paymentDetails = booking?.Payment;
            Assert.NotNull(booking);
            Assert.NotNull(booking?.TripResourceId);
            Assert.NotNull(paymentDetails);
            Assert.NotNull(paymentDetails?.TotalPaidAmount);
            Assert.Equal(4925.72m, paymentDetails?.TotalPaidAmount);
            Assert.NotNull(paymentDetails?.TotalPriceToBePaid);
            Assert.Equal(27554.32m, paymentDetails?.TotalPriceToBePaid);
            Assert.NotNull(paymentDetails?.BalanceDue);
            Assert.Equal(22628.6m, paymentDetails?.BalanceDue);
            Assert.NotNull(paymentDetails?.DueDate);
            Assert.Equal(DateOnly.Parse("2026-10-09"), paymentDetails?.DueDate);
            Assert.NotNull(paymentDetails?.PaidPercentage);
            Assert.Equal(18, paymentDetails?.PaidPercentage);
            Assert.NotNull(paymentDetails?.PaymentMinAmount);
            Assert.NotNull(paymentDetails?.PaymentMaxAmount);
            Assert.Equal(0m, paymentDetails?.PaymentMinAmount);
            Assert.Equal(22628.6m, paymentDetails?.PaymentMaxAmount);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_SuccessHoliday_ReturnsCheckinStatusPerPassenger()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996219",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Walker",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            var airlineProduct = booking.Product.FirstOrDefault(x => x.BookingType == "AIRLINE");
            Assert.NotNull(airlineProduct.Passengers);
            Assert.True(airlineProduct.Passengers.All(x => x.Slices != null));
            Assert.True(airlineProduct.Passengers.All(x => x.Slices.Any(y => y.CheckinStatus != null)));
        }

        [Fact]
        public async Task RetrieveHolidayBooking_TotalPriceToBePaidExceedsConfiguredMax_ClampsPaymentMaxAmount()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "9000000",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clamping",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.Payment);
            Assert.Equal(75000.0m, booking.Payment!.TotalPriceToBePaid);
            Assert.Equal(50000.0m, booking.Payment.PaymentMaxAmount);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ReturnsImportantInformation()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "9000000",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clamping",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking?.ImportantInformation);
            Assert.NotEmpty(booking.ImportantInformation!);
            Assert.All(booking.ImportantInformation!, info =>
            {
                Assert.NotNull(info);
                Assert.False(string.IsNullOrWhiteSpace(info.Priority));
                Assert.False(string.IsNullOrWhiteSpace(info.InformationText));
            });
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ReturnTicketAvailableDate_Success()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6997310",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Clarke",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.DocumentDetails?.TicketsDocumentAvailableTime);
            Assert.NotNull(booking.TravelDates?.DepartureDateUtc);
            Assert.Equal(LocalDateTime.FromDateTime(booking.TravelDates.DepartureDateUtc.Value.AddDays(-14)), booking.DocumentDetails.TicketsDocumentAvailableTime);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ForUnsupportedHolidayFlights_ReturnsSegmentLevelPnrLocator()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "9999998",
                DateOfBirth = DateOnly.Parse("1970-01-01"),
                Surname = "User",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);
            var airlineProduct = booking.Product!.FirstOrDefault(p => p.BookingType == "AIRLINE");
            Assert.NotNull(airlineProduct);
            Assert.False(airlineProduct.SupportedFlight);
            Assert.NotNull(airlineProduct.Slices);
            var slice = airlineProduct.Slices.FirstOrDefault();
            Assert.NotNull(slice);
            Assert.NotNull(slice.Segments);
            Assert.NotNull(slice.Segments);
            Assert.NotNull(slice.Segments.First()?.PnrLocator);
            Assert.Equal("EKTEST", slice.Segments.First()?.PnrLocator);
        }

        [Fact]
        public async Task RetrieveHolidayBooking_ForSupportedFlights_ReturnsProductLevelPnr_ForSegment()
        {
            var holidayQuery = _serviceProvider.GetService<HolidayQuery>()!;
            var request = new VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.TripsDetailsRequest()
            {
                BookingReference = "6996164",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayQuery.RetrieveHolidayBooking(request);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Product);
            var airlineProduct = booking.Product!.FirstOrDefault(p => p.BookingType == "AIRLINE");
            Assert.NotNull(airlineProduct);
            Assert.True(airlineProduct.SupportedFlight);
            Assert.NotNull(airlineProduct.Slices);
            var slice = airlineProduct.Slices.FirstOrDefault();
            Assert.NotNull(slice);
            Assert.NotNull(slice.Segments);
            Assert.NotNull(slice.Segments.First().PnrLocator);
            Assert.Equal(airlineProduct.Reference?.PnrLocator, slice.Segments.First().PnrLocator);
            Assert.Null(slice.Segments.First().ManageFlightLink);
        }

    }
}
