using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.Application;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests
{
    public sealed class RetrieveBookingServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string CommonSurname = "MismatchSurname";
        private readonly IServiceProvider _serviceProvider;

        public RetrieveBookingServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("Holidays:RetrieveBookingService:Url", "https://teste-ecs.tbox-test.vholsinternal.co.uk/cart-service/v2/cart/");
            ConfigReader.Set("Holidays:EntraScope", "testResource");
            ConfigReader.Set("Holidays:AuthorizationProvider", "AZURE");
            ConfigReader.Set("EapiMsClientID", Guid.NewGuid().ToString());
            ConfigReader.Set("EapiMSSecret", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:Authority", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:TenantId", Guid.NewGuid().ToString());

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOAuthTokenProvider, Core.Xunit.Azure.OAuthTokenProvider>()
                                                      .AddSingleton<IRetrieveBookingService, RetrieveBookingService>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task RetrieveBookingService()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "1234567890",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("1234567890"),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var bookingResponse = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(bookingResponse);
            Assert.NotNull(bookingResponse.Booking?.HolidayProducts);
        }


        [Fact]
        public async Task RetrieveBookingService_BookingSummary_ReturnsSuccess()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "1234567890",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("1234567890"),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var bookingResponse = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(bookingResponse);
            Assert.NotNull(bookingResponse.Booking?.BookingSummary);
            Assert.NotNull(bookingResponse.Booking?.BookingSummary.BrandCode);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.BrandName);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.Company);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.CompanyName);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.Division);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.DivisionName);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.BookingType);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.BookingTypeName);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.StoreLocation);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.StoreLocation.Value);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.IsVIPBooking);
            Assert.NotNull(bookingResponse.Booking.BookingSummary.LeadPassengerName);
        }


        [Fact]
        public async Task RetrieveBookingService_FlightHotelCarTransferGeneric_Products()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "1122334455",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("1122334455"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var bookingResponse = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(bookingResponse);
            Assert.Contains(bookingResponse.Booking.HolidayProducts, p => p.TbxProductType.Equals("HTL", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(bookingResponse.Booking.HolidayProducts, p => p.TbxProductType.Equals("FLT", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(bookingResponse.Booking.HolidayProducts, p => p.TbxProductType.Equals("GEN", StringComparison.OrdinalIgnoreCase));
            Assert.True(bookingResponse.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));
            Assert.True(bookingResponse.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x =>
            x.GenericProductDetails.IsElementGroupDetailsOverridden == false));
        }

        [Fact]
        public async Task RetrieveBookingService_CruiseBooking_ReturnsBadRequest()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6669809",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = CommonSurname,
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var response = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Null(response.Booking);
            Assert.Equal(400, response.Status);
            Assert.Contains(response.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode);
        }

        [Fact]
        public async Task RetrieveBookingService_Disney_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996245",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6996245"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN");

            var genProducts = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN").First();
            Assert.Equal("14-Day Disney Magic Ticket", genProducts.Summary.Name);
            Assert.Equal("Disney - Park Tickets", genProducts.Summary.PrimaryAttribute);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_Disney_2_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996566",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6996566"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY");
            var disneyProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY").FirstOrDefault();
            Assert.Equal("14-Day Disney Magic Ticket", disneyProduct.Summary.Name);
            Assert.Equal("Disney - Park Tickets", disneyProduct.Summary.PrimaryAttribute);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_NormaliseName_TrailingSpace_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996566",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = $"{GetLeadTravellerSurname("6996566")} ", //trailing whitespace in surName 
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY");
            var disneyProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY").FirstOrDefault();
            Assert.Equal("14-Day Disney Magic Ticket", disneyProduct.Summary.Name);
            Assert.Equal("Disney - Park Tickets", disneyProduct.Summary.PrimaryAttribute);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_NormaliseName_WhitespaceFrontBack_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996566",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = $" \t{GetLeadTravellerSurname("6996566")} \r\n \t", //trailing whitespace in surName 
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY");
            var disneyProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.Summary.ElementGroup == "DISNEY").FirstOrDefault();
            Assert.Equal("14-Day Disney Magic Ticket", disneyProduct.Summary.Name);
            Assert.Equal("Disney - Park Tickets", disneyProduct.Summary.PrimaryAttribute);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task Retrieve_AdvanceBooking_Destination()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6824290",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6824290").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "ADVANCE");
            Assert.Equal("San Francisco", booking.Booking.HolidayPackageDetails.Destination.CityName);
            Assert.Equal("USA", booking.Booking.HolidayPackageDetails.Destination.CountryName);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task Retrieve_AdvanceCancelledBooking_Destination()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6822307",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6822307").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.DoesNotContain(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "ADVANCE");
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN");
            Assert.Equal("Orlando", booking.Booking.HolidayPackageDetails.Destination.CityName);
            Assert.Equal("USA", booking.Booking.HolidayPackageDetails.Destination.CountryName);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));
        }

        [Fact]
        public async Task RetrieveBookingService_Core_CarProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6833336",
                DateOfBirth = DateOnly.Parse("1996-06-12"),
                Surname = GetLeadTravellerSurname("6833336").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            var carProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == false && x.TbxProductType == "CAR").FirstOrDefault();
            Assert.NotNull(carProduct);
            Assert.NotNull(carProduct.CarDetails);
            Assert.Equal("Buick - Encore or similar - Compact SUV", carProduct.CarDetails.Name);
            Assert.False(carProduct.CarDetails.VipBooking);
            Assert.Equal(DateTime.Parse("2026-05-20T16:25:00"), carProduct.CarDetails.StartDate);
            Assert.Equal(1, carProduct.CarDetails.NumberOfCars);
            Assert.Equal("CAR_HIRE", carProduct.CarDetails.ContractType);
            Assert.Equal("CFAR", carProduct.CarDetails.Class);
        }

        [Fact]
        public async Task RetrieveBookingService_Verify_CarDetail()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6833025",
                DateOfBirth = DateOnly.Parse("1999-01-01"),
                Surname = "test",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            var carProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == false && x.TbxProductType == "CAR").FirstOrDefault();
            Assert.NotNull(carProduct);
            Assert.NotNull(carProduct.CarDetails);
            Assert.False(string.IsNullOrWhiteSpace(carProduct.CarDetails?.Name));
            Assert.Equal("Nissan - Versa or similar - Compact 4 door", carProduct.CarDetails?.Name);
            Assert.Equal("Nissan", carProduct.CarDetails?.Make);
            Assert.Equal("Versa or similar", carProduct.CarDetails?.Model);
            Assert.Equal("CDAR", carProduct.CarDetails?.Class);
            Assert.Equal("Compact 4 door", carProduct.CarDetails?.ClassName);
            Assert.Equal(4, carProduct.CarDetails?.DoorCount);
            Assert.Equal(true, carProduct.CarDetails?.AirConditioned);
            Assert.Equal(DateTime.Parse("2026-09-21T16:25:00"), carProduct.CarDetails?.StartDate);
            Assert.Equal(DateTime.Parse("2026-10-07T16:00:00"), carProduct.CarDetails?.EndDate);
            Assert.Equal(" ", carProduct.CarDetails?.SippCode);

            // Product details
            Assert.NotNull(carProduct.CarDetails?.CarProduct);
            Assert.Equal(4130, carProduct.CarDetails?.CarProduct?.Id);
            Assert.False(string.IsNullOrWhiteSpace(carProduct.CarDetails?.CarProduct?.Name));
            Assert.Equal("Hertz Go ", carProduct.CarDetails?.CarProduct?.Name);

            // Supplier information
            Assert.NotNull(carProduct.SupplierDetails);
            Assert.False(string.IsNullOrWhiteSpace(carProduct.SupplierDetails?.SupplierPaymentInfo?.SupplierName));
            Assert.Equal("Hertz", carProduct.SupplierDetails?.SupplierPaymentInfo?.SupplierName);
            Assert.Equal("L1264130821", carProduct.SupplierDetails?.SupplierCommunicationInfo?.ConfirmationNo);

            // Traveller composition
            Assert.NotNull(carProduct.TravellersComposition);
            Assert.Equal(2, carProduct.TravellersComposition?.AdultCount);
            Assert.Equal(0, carProduct.TravellersComposition?.ChildCount);
            Assert.Equal(0, carProduct.TravellersComposition?.InfantCount);
            Assert.True(carProduct.TravellersComposition?.Travellers.Any(x => x.LeadDriver == true));

            // Locations
            Assert.Equal("Hertz\n\n*Car pick up/drop off is A side*\n\nOrlando International Airport\n1 Airport Blvd\nORLANDO\nFL 32812\n\nOpening times\n 24/7", carProduct.CarDetails.SelectedPickupLocation.Address);
            Assert.Equal("Hertz\n\n*Car pick up/drop off is A side*\n\nOrlando International Airport\n1 Airport Blvd\nORLANDO\nFL 32812\n\nOpening times\n 24/7", carProduct.CarDetails.SelectedReturnLocation.Address);
        }

        [Fact]
        public async Task RetrieveBookingService_Ancillary_CarProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996164",
                DateOfBirth = DateOnly.Parse("1977-08-12"),
                Surname = GetLeadTravellerSurname("6996164").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            var carProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == false && x.TbxProductType == "CAR").FirstOrDefault();
            Assert.NotNull(carProduct);
            Assert.NotNull(carProduct.CarDetails);
            Assert.Equal("Ford - Edge or similar - Standard SUV", carProduct.CarDetails.Name);
            Assert.False(carProduct.CarDetails.VipBooking);
            Assert.Equal(DateTime.Parse("2026-10-08T19:10:00"), carProduct.CarDetails.StartDate);
            Assert.Equal(1, carProduct.CarDetails.NumberOfCars);
            Assert.Null(carProduct.CarDetails.ContractType);
            Assert.Equal("SFAR", carProduct.CarDetails.Class);
        }

        [Fact]
        public async Task RetrieveBookingService_Universal_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "7004639",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("7004639"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "GEN" && x.Summary.ElementGroup == "USAEXC");
            var universalProduct = booking.Booking.HolidayProducts.Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.Summary.ElementGroup == "USAEXC").FirstOrDefault();
            Assert.Equal("04-10-2026", universalProduct.Summary.Name);
            Assert.Equal("Universal 1 Day, 2nd Day Free Ticket", universalProduct.Summary.Highlight);
            Assert.NotNull(universalProduct.TravellersComposition);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_HTlFLTOnly_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6661908",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6661908"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking);
            Assert.NotNull(booking.Booking?.HolidayPackageDetails?.Destination?.CityName);
            Assert.True(booking.Booking.Travellers.Count() > 0);
            Assert.True(booking.Booking.Summary.TravelDates.StartDate.HasValue);
            Assert.True(booking.Booking.Summary.TravelDates.EndDate.HasValue);

            Assert.NotNull(booking.Booking.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "HTL");
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "FLT");


            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails.RoomDetails.Count() > 0);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.FlightDetails);

            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.HotelDetails);
            Assert.Equal(2, booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.Count());
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.FlightNumber != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(a => a.Segments).Select(x => x.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.IsBookingSourceAir4 == true);
            var flightProduct = booking.Booking.HolidayProducts.First(x => x.TbxProductType == "FLT");
            var terminalNames = flightProduct.FlightDetails.Sectors
                .SelectMany(s => s.Segments)
                .SelectMany(s => s.Legs ?? Array.Empty<Leg>())
                .SelectMany(l => new[] { l.Origin?.TerminalName, l.Destination?.TerminalName })
                .Where(t => !t.IsNullOrWhiteSpace())
                .ToList();

            Assert.Contains("Terminal 4", terminalNames);
            Assert.Contains("Terminal 3", terminalNames);


            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.FlightDetails);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.GenericProductDetails);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x =>
            x.GenericProductDetails.IsElementGroupDetailsOverridden == false));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Name != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Highlight != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.PrimaryAttribute != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.GenericProductDetails != null));

            //Assert that in all the holiday products there should at least be one details populated out of car, hotel flight, cruise or generic 
            Assert.True(booking.Booking.HolidayProducts.All(x => x.HotelDetails != null || x.FlightDetails != null || x.GenericProductDetails != null || x.CarDetails != null || x.CruiseDetails != null));
        }

        [Fact]
        public async Task RetrieveBookingService_RequestDOBMismatch_failure()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6661809",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6661809"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking.Errors);
            Assert.True(booking.Errors.Any(e => e.Code == Constants.Application.HolidaysApiLeadPaxValidationErrorCode));
            Assert.True(booking.Errors.Any(e => e.DeveloperMessage.Contains("InvalidRequestedParameters")));
        }


        [Fact]
        public async Task RetrieveListBookingService_FlightHotelCarTransferGeneric_Products()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "1122334455",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("1122334455"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.Contains(listResponse.TripListResponse, x => x.TripDetails.Booking.Id == "1122334455");
            Assert.True(listResponse.TripListResponse.First().TripDetails.Status == (int)System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task RetrieveBookingService_TboxErrorMessage()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "6999399",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.Null(listResponse.TripListResponse.FirstOrDefault()?.TripDetails?.Booking);
            Assert.Equal(400, listResponse.TripListResponse.FirstOrDefault()?.TripDetails?.Status);
            Assert.Contains(listResponse.TripListResponse.FirstOrDefault()?.TripDetails?.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.TboxErrorCode);
        }


        [Fact]
        public async Task RetrieveBookingService_TboxErrorMessageandCruiseErrorMessage()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new()
                    {
                           BookingReference = "6999399",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                 new()
                 {
                           BookingReference = "6669809",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.All(listResponse.TripListResponse, x => Assert.Null(x.TripDetails?.Booking));
            Assert.All(listResponse.TripListResponse, x => Assert.Equal(400, x.TripDetails?.Status));
            Assert.All(listResponse.TripListResponse.Where(x => x.TripSearchRequest.BookingReference == "6999399"), x => Assert.Contains(x.TripDetails?.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.TboxErrorCode));
            Assert.All(listResponse.TripListResponse.Where(x => x.TripSearchRequest.BookingReference == "6669809"), x => Assert.Contains(x.TripDetails?.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode));
        }

        [Fact]
        public async Task RetrieveBookingService_TboxErrorMessageandOneSuccessBooking()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new()
                    {
                           BookingReference = "6999399",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                 new()
                 {
                           BookingReference = "6661908",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("6661908"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            var tboxErrorBookingResponse = listResponse.TripListResponse.Where(x => x.TripSearchRequest.BookingReference == "6999399");

            Assert.All(tboxErrorBookingResponse, x => Assert.Null(x.TripDetails?.Booking));
            Assert.All(tboxErrorBookingResponse, x => Assert.Equal(400, x.TripDetails?.Status));
            Assert.All(tboxErrorBookingResponse, x => Assert.Contains(x.TripDetails?.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.TboxErrorCode));

            var successBookingResponse = listResponse.TripListResponse.FirstOrDefault(x => x.TripSearchRequest.BookingReference == "6661908")?.TripDetails;
            Assert.NotNull(successBookingResponse);
            Assert.NotNull(successBookingResponse.Booking);
            Assert.NotNull(successBookingResponse.Booking?.HolidayPackageDetails?.Destination?.CityName);
            Assert.True(successBookingResponse.Booking.Travellers.Count() > 0);
            Assert.True(successBookingResponse.Booking.Summary.TravelDates.StartDate.HasValue);
            Assert.True(successBookingResponse.Booking.Summary.TravelDates.EndDate.HasValue);

            Assert.NotNull(successBookingResponse.Booking.HolidayProducts);
            Assert.Contains(successBookingResponse.Booking.HolidayProducts, x => x.TbxProductType == "HTL");
            Assert.Contains(successBookingResponse.Booking.HolidayProducts, x => x.TbxProductType == "FLT");
            Assert.True(successBookingResponse.Booking.HolidayProducts.All(x => x.HotelDetails != null || x.FlightDetails != null || x.GenericProductDetails != null || x.CarDetails != null || x.CruiseDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingListService_CruiseBooking_ReturnsBadRequest()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "6669809",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS
                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);
            Assert.True(listResponse.TripListResponse.IsEmpty);
        }

        [Fact]
        public async Task RetrieveListBookingService_ReturnTransferAncillaries_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "7003539",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("7003539"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);

            var tripResponse = listResponse.TripListResponse.FirstOrDefault(x => x.TripDetails.Booking.Id == "7003539");
            Assert.NotNull(tripResponse);

            var trsProduct = tripResponse.TripDetails.Booking.HolidayProducts.Where(x => x.TbxProductType == "TRS");
            Assert.Equal(2, trsProduct.Count());
            Assert.True(trsProduct.All(x => x.IsAncillaryProduct == true));
            Assert.True(trsProduct.All(x => x.GenericProductDetails != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.PickupPoints.Any()));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.DropoffPoints.Any()));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.SelectedPickupPoint != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.SelectedDropoffPoint != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.ElementGroup == "TRANSFER"));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.ElementGroupName == "Transfer"));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.IsElementGroupDetailsOverridden == true));
            Assert.True(listResponse.TripListResponse.All(trip => trip.TripDetails.Booking.HolidayProducts.All(p => p.HotelDetails != null || p.FlightDetails != null || p.GenericProductDetails != null || p.CarDetails != null || p.CruiseDetails != null)));

        }

        [Fact]
        public async Task RetrieveListBookingService_OutsideTransfer_ReturnTransferInsideAncillaries_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6997721",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = GetLeadTravellerSurname("6997721"),
                    Using = RequestOptionCodeType.HOLIDAYS


                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);

            var trsProduct = response.Booking.HolidayProducts.Where(x => x.TbxProductType == "TRS");
            Assert.Equal(2, trsProduct.Count());
            Assert.True(trsProduct.All(x => x.IsAncillaryProduct == true));
            Assert.True(trsProduct.All(x => x.GenericProductDetails != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.PickupPoints.Any()));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.DropoffPoints.Any()));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.SelectedPickupPoint != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.SelectedDropoffPoint != null));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.ElementGroup == "TRANSFER"));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.ElementGroupName == "Transfer"));
            Assert.True(trsProduct.All(x => x.GenericProductDetails.IsElementGroupDetailsOverridden == true));
        }

        [Fact]
        public async Task RetrieveListBookingService_BookingUnavailable()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "111111",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.True(listResponse.TripListResponse.Count() > 0);
            Assert.True(listResponse.TripListResponse.First().TripSearchRequest.BookingReference == "111111");
            Assert.True(listResponse.TripListResponse.First().TripDetails.Status == (int)System.Net.HttpStatusCode.BadRequest);
            Assert.True(listResponse.TripListResponse.First().TripDetails.Booking == null);

        }

        [Fact]
        public async Task RetrieveListBookingService_BookingOKBadRequest()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new TripsDetailsRequest()
                    {
                           BookingReference = "111111",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = CommonSurname,
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                new TripsDetailsRequest()
                    {
                           BookingReference = "1122334455",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("1122334455"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.True(listResponse.TripListResponse.Count() == 2);
            Assert.True(listResponse.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "111111").TripDetails.Booking == null);
            Assert.True(listResponse.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "111111").TripDetails.Status == (int)System.Net.HttpStatusCode.BadRequest);

            Assert.True(listResponse.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "1122334455").TripDetails.Booking != null);
            Assert.True(listResponse.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "1122334455").TripDetails.Status == (int)System.Net.HttpStatusCode.OK);
            Assert.Equal("1122334455", listResponse.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "1122334455").TripDetails.Booking.Id);
        }

        [Fact]
        public async Task RetrieveBookingService_HolidayReturnDateIs7DaysInPast_ReturnFailure()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6999382",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = CommonSurname,
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Null(response.Booking);
            Assert.Equal(400, response.Status);
            Assert.Contains(response.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode);
        }

        [Fact]
        public async Task RetrieveListBookingService_HandlesExpiredBookings()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new()
                    {
                           BookingReference = "6999383",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("6999383"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                new()
                    {
                           BookingReference = "6661909",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = GetLeadTravellerSurname("6661909"),
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.All(listResponse.TripListResponse, item => Assert.Null(item.TripDetails.Booking));
            Assert.All(listResponse.TripListResponse, item => Assert.Equal(400, item.TripDetails.Status));
        }

        [Fact]
        public async Task RetrieveBookingService_QuoteBooking_ReturnsQuoteStatus()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6836033",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = CommonSurname,
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Null(response.Booking);
            Assert.Equal(400, response.Status);
            Assert.Contains(response.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode);
        }

        [Fact]
        public async Task RetrieveBookingService_CancelledBooking_ReturnFailure()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6836034",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = CommonSurname,
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Null(response.Booking);
            Assert.Equal(400, response.Status);
            Assert.Contains(response.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode);
        }

        [Fact]
        public async Task RetrieveBookingService_ExpiredBooking_ReturnFailure()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6999382",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = CommonSurname,
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Null(response.Booking);
            Assert.Equal(400, response.Status);
            Assert.Contains(response.Errors, error => error.Code == VAA.EAPI.Microservices.Retail.Holidays.Constants.Application.HolidaysApiRequestValidationErrorCode);
        }

        [Fact]
        public async Task RetrieveBookingService_ReturnCruiseBookingSuccess()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6908893",
                    DateOfBirth = new DateOnly(1996, 2, 20),
                    Surname = GetLeadTravellerSurname("6908893"),
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);
            Assert.Null(response.Errors);
            Assert.True(response.Booking.HolidayProducts.Any(x => x.TbxProductType == "CRU"));
        }

        [Fact]
        public async Task RetrieveBookingService_SupportAMDFlightWithVSCarrier_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var dob = DateOnly.TryParse("1984-09-27", out var parsedDob) ? parsedDob : default;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6995389",
                DateOfBirth = dob,
                Surname = GetLeadTravellerSurname("6995389"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking);
            Assert.NotNull(booking.Booking?.HolidayPackageDetails?.Destination?.CityName);
            Assert.True(booking.Booking.Travellers.Count() > 0);
            Assert.True(booking.Booking.Summary.TravelDates.StartDate.HasValue);
            Assert.True(booking.Booking.Summary.TravelDates.EndDate.HasValue);

            Assert.NotNull(booking.Booking.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "HTL");
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "FLT");


            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails.RoomDetails.Count() > 0);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.FlightDetails);

            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.HotelDetails);
            Assert.Equal(2, booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.Count());
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.FlightNumber != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(a => a.Segments).Select(x => x.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.IsBookingSourceAir4 == true);
            var flightProduct = booking.Booking.HolidayProducts.First(x => x.TbxProductType == "FLT");
            var terminalNames = flightProduct.FlightDetails.Sectors
                .SelectMany(s => s.Segments)
                .SelectMany(s => s.Legs ?? Array.Empty<Leg>())
                .SelectMany(l => new[] { l.Origin?.TerminalName, l.Destination?.TerminalName })
                .Where(t => !t.IsNullOrWhiteSpace())
                .ToList();

            Assert.Contains("Terminal 4", terminalNames);
            Assert.Contains("Terminal 3", terminalNames);


            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.FlightDetails);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.GenericProductDetails);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x =>
            x.GenericProductDetails.IsElementGroupDetailsOverridden == false));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Name != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Highlight != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.PrimaryAttribute != null));
            Assert.True(booking.Booking.HolidayProducts.All(x => x.HotelDetails != null || x.FlightDetails != null || x.GenericProductDetails != null || x.CarDetails != null || x.CruiseDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_SupportAMDFlightWithNonVSCarrier_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var dob = DateOnly.TryParse("1982-09-14", out var parsedDob) ? parsedDob : default;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6995709",
                DateOfBirth = dob,
                Surname = GetLeadTravellerSurname("6995709"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking);
            Assert.True(booking.Booking.Travellers.Count() > 0);
            Assert.True(booking.Booking.Summary.TravelDates.StartDate.HasValue);
            Assert.True(booking.Booking.Summary.TravelDates.EndDate.HasValue);

            Assert.NotNull(booking.Booking.HolidayProducts);
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "HTL");
            Assert.Contains(booking.Booking.HolidayProducts, x => x.TbxProductType == "FLT");


            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails.RoomDetails.Count() > 0);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "HTL").FirstOrDefault()?.FlightDetails);

            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.GenericProductDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.HotelDetails);
            Assert.Equal(3, booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.Count());
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.FlightNumber != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(a => a.Segments).Select(x => x.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.Sectors.SelectMany(x => x.Segments).Select(a => a.OperatingCarrier).All(p => p.CarrierCode != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "FLT").FirstOrDefault()?.FlightDetails.IsBookingSourceAir4 == false);
            var flightProduct = booking.Booking.HolidayProducts.First(x => x.TbxProductType == "FLT");
            var terminalNames = flightProduct.FlightDetails.Sectors
                .SelectMany(s => s.Segments)
                .SelectMany(s => s.Legs ?? Array.Empty<Leg>())
                .SelectMany(l => new[] { l.Origin?.TerminalName, l.Destination?.TerminalName })
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            Assert.Contains("Terminal 2", terminalNames);
            Assert.Contains("Terminal 3", terminalNames);


            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.HotelDetails);
            Assert.Null(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.FlightDetails);
            Assert.NotNull(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").FirstOrDefault()?.GenericProductDetails);
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x =>
            x.GenericProductDetails.IsElementGroupDetailsOverridden == false));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Name != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.Highlight != null));
            Assert.True(booking.Booking.HolidayProducts.Where(x => x.TbxProductType == "GEN").All(x => x.Summary.PrimaryAttribute != null));
            Assert.True(booking.Booking.HolidayProducts.All(x => x.HotelDetails != null || x.FlightDetails != null || x.GenericProductDetails != null || x.CarDetails != null || x.CruiseDetails != null));

        }

        [Fact]
        public async Task RetrieveBookingService_BookingWithQuoteProductStatus_FilterProducts()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6874036",
                    DateOfBirth = new DateOnly(1972, 1, 9),
                    Surname = "TestLastName",
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);
            Assert.DoesNotContain(response?.Booking?.HolidayProducts ?? [], product => product.Summary.ProductStatus == "QUOTE");
        }

        [Fact]
        public async Task RetrieveBookingService_BookingWithMultipleSegments_ReturnSlices()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6874036",
                    DateOfBirth = new DateOnly(1972, 1, 9),
                    Surname = "TestLastName",
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);
            var flightDetails = response.Booking.HolidayProducts.Where(product => product.FlightDetails != null).Select(product => product.FlightDetails);
            Assert.All(response.Booking.HolidayProducts.Where(product => product.FlightDetails != null), p => Assert.NotNull(p.FlightDetails?.AirlineLocator?.PnrLocator));
            Assert.NotNull(flightDetails);
            Assert.NotEmpty(flightDetails);
            Assert.All(flightDetails, details => Assert.NotNull(details.Sectors));
            Assert.Equal(4, flightDetails.SelectMany(details => details.Sectors).Count());
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Id));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.ScheduledDepartureLocal));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.ScheduledArrivalLocal));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Origin));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Destination));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.True(sector.Segments.Count() >= 1));
            Assert.Contains(flightDetails.SelectMany(details => details.Sectors), sec => sec.Segments.Count() > 1);

            var sectors = flightDetails.SelectMany(details => details.Sectors).ToList();
            Assert.All(sectors, sector => Assert.Matches("^PT\\d+H\\d+M$", sector.TotalSliceDuration));
            Assert.All(sectors.SelectMany(sector => sector.Segments).SelectMany(segment => segment.Legs), leg => Assert.Null(leg.OnAirDuration));


        }

        [Fact]
        public async Task RetrieveBookingService_BookingWithMultipleFareInfo_ReturnSlices()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6874038",
                    DateOfBirth = new DateOnly(1972, 1, 9),
                    Surname = "TestLastName",
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);
            var flightDetails = response.Booking.HolidayProducts.Where(product => product.FlightDetails != null).Select(product => product.FlightDetails);
            Assert.All(response.Booking.HolidayProducts.Where(product => product.FlightDetails != null), p => Assert.NotNull(p.FlightDetails?.AirlineLocator?.PnrLocator));
            Assert.NotNull(flightDetails);
            Assert.NotEmpty(flightDetails);
            Assert.All(flightDetails, details => Assert.NotNull(details.Sectors));
            Assert.Equal(3, flightDetails.SelectMany(details => details.Sectors).Count());
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Id));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.ScheduledDepartureLocal));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.ScheduledArrivalLocal));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Origin));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.NotNull(sector.Destination));
            Assert.All(flightDetails.SelectMany(details => details.Sectors), sector => Assert.True(sector.Segments.Count() >= 1));
            Assert.Contains(flightDetails.SelectMany(details => details.Sectors), sec => sec.Segments.Count() > 1);
            var leg = flightDetails.SelectMany(details => details.Sectors).SelectMany(sector => sector.Segments).SelectMany(segment => segment.Legs).LastOrDefault();
            Assert.NotNull(leg);
        }

        [Fact]
        public async Task RetrieveBookingService_BookingWithConnectionSplitAcrossFareInfo_MergesIntoSingleSector()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequest =
                new TripsDetailsRequest()
                {
                    BookingReference = "6874038",
                    DateOfBirth = new DateOnly(1972, 1, 9),
                    Surname = "TestLastName",
                    Using = RequestOptionCodeType.HOLIDAYS
                };

            var response = await retrieveBookingService.Handle(retrieveBookingRequest, CancellationToken.None);

            Assert.NotNull(response);
            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);

            var sectors = response.Booking.HolidayProducts
                .Where(product => product.FlightDetails != null)
                .SelectMany(product => product.FlightDetails!.Sectors ?? [])
                .ToList();

            var inboundSectors = sectors
                .Where(sector => sector.Origin?.Code == "HNL" && sector.Destination?.Code == "LHR")
                .ToList();

            Assert.Single(inboundSectors);

            var inbound = inboundSectors[0];
            var segments = inbound.Segments.ToList();

            Assert.Equal(2, segments.Count);
            Assert.Equal("HNL", segments[0].Origin?.Code);
            Assert.Equal("LAX", segments[0].Destination?.Code);
            Assert.Equal("LAX", segments[1].Origin?.Code);
            Assert.Equal("LHR", segments[1].Destination?.Code);
            Assert.Null(inbound.TotalSliceDuration);
        }

        [Fact]
        public async Task RetrieveBookingService_MultiLegBooking_MapsEachLegOriginAndDestination()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest
            {
                BookingReference = "7007912",
                DateOfBirth = new DateOnly(1985, 1, 15),
                Surname = "Sample",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var response = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);

            var legs = response.Booking.HolidayProducts
                .Where(product => product.FlightDetails != null)
                .SelectMany(product => product.FlightDetails!.Sectors ?? [])
                .SelectMany(sector => sector.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? [])
                .ToList();

            Assert.Collection(
                legs,
                leg =>
                {
                    Assert.Equal(("LHR", "BGI"), (leg.Origin?.Code, leg.Destination?.Code));
                    Assert.Null(leg.ScheduledDepartureLocal);
                    Assert.Null(leg.ScheduledArrivalLocal);
                },
                leg =>
                {
                    Assert.Equal(("BGI", "GND"), (leg.Origin?.Code, leg.Destination?.Code));
                    Assert.Null(leg.ScheduledDepartureLocal);
                    Assert.Null(leg.ScheduledArrivalLocal);
                },
                leg =>
                {
                    Assert.Equal(("GND", "BGI"), (leg.Origin?.Code, leg.Destination?.Code));
                    Assert.Null(leg.ScheduledDepartureLocal);
                    Assert.Null(leg.ScheduledArrivalLocal);
                },
                leg =>
                {
                    Assert.Equal(("BGI", "LHR"), (leg.Origin?.Code, leg.Destination?.Code));
                    Assert.Null(leg.ScheduledDepartureLocal);
                    Assert.Null(leg.ScheduledArrivalLocal);
                });
        }

        [Fact]
        public void RetrieveBookingService_MissingDeparture_DoesNotPromoteTouchdownToOrigin()
        {
            var source = JObject.Parse("""
                {
                  "departure": " ",
                  "arrival": "GND",
                  "departureDateTime": "2027-01-02T09:40:00",
                  "arrivalDateTime": "2027-01-02T17:05:00",
                  "touchdownDetails": [
                    { "location": "BGI" }
                  ]
                }
                """);

            var segment = (Segment)source;
            var leg = Assert.Single(segment.Legs);

            Assert.Equal(" ", leg.Origin?.Code);
            Assert.Equal("GND", leg.Destination?.Code);
        }

        [Fact]
        public async Task RetrieveBookingService_SingleLegBooking_MapsDepartureArrivalOriginAndDestination()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest
            {
                BookingReference = "6874036",
                DateOfBirth = new DateOnly(1972, 1, 9),
                Surname = "TestLastName",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var response = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(response.Booking);
            Assert.Equal(200, response.Status);

            var legs = response.Booking.HolidayProducts
                .Where(product => product.FlightDetails != null)
                .SelectMany(product => product.FlightDetails!.Sectors ?? [])
                .SelectMany(sector => sector.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? []);

            Assert.Contains(legs, leg =>
                leg.Origin?.Code == "LAX" &&
                leg.Destination?.Code == "HNL" &&
                leg.ScheduledDepartureLocal == new DateTime(2026, 9, 30, 13, 10, 0) &&
                leg.ScheduledArrivalLocal == new DateTime(2026, 9, 30, 16, 5, 0));
        }


        [Fact]
        public async Task RetrieveBookingService_PaymentHistory_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6833336",
                DateOfBirth = DateOnly.Parse("1996-06-12"),
                Surname = GetLeadTravellerSurname("6833336").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking.Payment);
            Assert.NotNull(booking.Booking.Payment.PastPayments);
            Assert.Contains(booking.Booking.Payment.PastPayments, payment => payment.CurrencyAmount == 350.00m
            && payment.PaymentCurrency == "GBP"
            && payment.RealisedDate == DateTime.Parse("2025-02-06T15:47:15")
            && payment.PaymentMethodCode == "CRCD"
            && payment.Realised == true
            && payment.ReceiptDate == DateTime.Parse("2025-02-06T15:47:15"));
            Assert.All(booking.Booking.Payment.PastPayments, payment => Assert.True(payment.CurrencyAmount is > 0m));
        }


        [Fact]
        public async Task RetrieveBookingService_Payment_Validate()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6833336",
                DateOfBirth = DateOnly.Parse("1996-06-12"),
                Surname = GetLeadTravellerSurname("6833336").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            var payment = booking.Booking?.Payment;
            Assert.NotNull(payment);
            Assert.NotNull(payment?.BalanceDue);
            Assert.Equal(7671.52m, payment?.BalanceDue);
            Assert.NotNull(payment?.TotalPaidAmount);
            Assert.Equal(350.00m, payment?.TotalPaidAmount);
            Assert.NotNull(payment?.TotalPriceToBePaid);
            Assert.Equal(8021.52m, payment?.TotalPriceToBePaid);
            Assert.NotNull(payment?.CurrencyCode);
            Assert.Equal("GBP", payment?.CurrencyCode);
        }

        [Fact]
        public async Task RetrieveBookingService_Package_Validate()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6833336",
                DateOfBirth = DateOnly.Parse("1996-06-12"),
                Surname = GetLeadTravellerSurname("6833336").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            var package = booking.Booking?.HolidayPackageDetails;
            Assert.NotNull(package);
            Assert.NotNull(package?.BalanceDueDate);
            Assert.Equal(DateOnly.Parse("2026-02-25"), package?.BalanceDueDate);
            Assert.NotNull(package?.FirmDueDate);
            Assert.NotNull(package?.DefiniteDueDate);

        }

        [Fact]
        public async Task RetrieveBookingService_PaymentHistoryWithCRCDRefund_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6468392",
                DateOfBirth = DateOnly.Parse("1955-09-01"),
                Surname = GetLeadTravellerSurname("6468392").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking.Payment);
            Assert.NotNull(booking.Booking.Payment.PastPayments);
            var payment = booking.Booking.Payment.PastPayments.FirstOrDefault(x => x.PaymentMethodCode == "LP");
            Assert.NotNull(payment);
            Assert.Equal("GBP", payment.PaymentCurrency);
            Assert.Multiple(() =>
            {
                var refundPayment = booking.Booking.Payment.PastPayments.LastOrDefault(p => p.PaymentMethodCode == Constants.Application.CreditCardPaymentMethod && p.TransactionType == PaymentTransactionType.Refund);
                Assert.NotNull(refundPayment);
                Assert.Equal(PaymentTransactionType.Refund, booking.Booking.Payment.PastPayments.Last().TransactionType);
                Assert.True(booking.Booking.Payment.PastPayments.Last().CurrencyAmount is > 0m);
            });

        }

        [Fact]
        public async Task RetrieveBookingService_PaymentHistoryWithBTRFRefund_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6465965",
                DateOfBirth = DateOnly.Parse("1975-04-16"),
                Surname = GetLeadTravellerSurname("6465965").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking.Payment);
            Assert.NotNull(booking.Booking.Payment.PastPayments);
            Assert.Multiple(() =>
            {
                Assert.Contains(booking.Booking.Payment.PastPayments, p => p.PaymentMethodCode == "BTRF" && p.TransactionType == PaymentTransactionType.Refund);
                Assert.Equal(PaymentTransactionType.Refund, booking.Booking.Payment.PastPayments.Last().TransactionType);
                Assert.All(booking.Booking.Payment.PastPayments.Where(predicate => predicate.PaymentMethodCode == "BTRF" && predicate.TransactionType == PaymentTransactionType.Refund), pay => Assert.True(pay.CurrencyAmount is > 0m));
            });

        }

        [Fact]
        public async Task RetrieveBookingService_PaymentHistoryWithOtherCorrection_Success()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6619029",
                DateOfBirth = DateOnly.Parse("1966-09-21"),
                Surname = GetLeadTravellerSurname("6619029").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };


            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking.Payment);
            Assert.NotNull(booking.Booking.Payment.PastPayments);
            Assert.Multiple(() =>
            {
                Assert.Contains(booking.Booking.Payment.PastPayments, p => p.PaymentMethodCode == "IBTX" && p.TransactionType == PaymentTransactionType.Correction);
                Assert.Equal(PaymentTransactionType.Correction, booking.Booking.Payment.PastPayments.Last().TransactionType);
                Assert.All(booking.Booking.Payment.PastPayments.Where(predicate => predicate.PaymentMethodCode == "IBTX" && predicate.TransactionType == PaymentTransactionType.Correction), pay => Assert.True(pay.CurrencyAmount is > 0m));
            });

        }

        private static string GetLeadTravellerSurname(string bookingReference)
        {
            var mocksDirectory = Path.Combine(AppContext.BaseDirectory, ".mocks");

            var matchingFiles = Directory.EnumerateFiles(mocksDirectory, $"*{bookingReference}*.mock")
                                         .Where(path => path.Contains("retrieveBooking", StringComparison.OrdinalIgnoreCase))
                                         .Take(2)
                                         .ToList();

            var mockFilePath = matchingFiles.Count switch
            {
                1 => matchingFiles[0],
                0 => null,
                _ => throw new InvalidOperationException($"Multiple retrieveBooking mock files found for booking reference '{bookingReference}': {string.Join(", ", matchingFiles.Select(Path.GetFileName))}")
            };

            if (mockFilePath is null)
            {
                throw new FileNotFoundException($"Could not locate retrieveBooking mock file for booking reference '{bookingReference}'.", mocksDirectory);
            }

            var mockText = File.ReadAllText(mockFilePath);
            var jsonText = ExtractResponseJson(mockText);

            using var doc = JsonDocument.Parse(jsonText, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            var root = doc.RootElement;
            var travellers = root.GetProperty("data")[0].GetProperty("travellers");

            foreach (var traveller in travellers.EnumerateArray())
            {
                if (traveller.TryGetProperty("lead", out var lead) && lead.GetBoolean())
                {
                    var lastName = traveller.GetProperty("profile").GetProperty("lastName").GetString() ?? string.Empty;
                    return lastName.Trim();
                }
            }

            throw new InvalidOperationException($"No lead traveller found in retrieveBooking mock response for booking reference '{bookingReference}'.");
        }

        private static string ExtractResponseJson(string mockText)
        {
            var responseIndex = mockText.IndexOf("###Response", StringComparison.OrdinalIgnoreCase);
            if (responseIndex < 0)
            {
                throw new InvalidOperationException("Mock file does not contain a ###Response section.");
            }

            var jsonStart = mockText.IndexOf('{', responseIndex);
            if (jsonStart < 0)
            {
                throw new InvalidOperationException("Mock file response does not appear to contain a JSON object.");
            }

            return mockText.Substring(jsonStart);
        }

        [Fact]
        public async Task RetrieveBookingService_HotelDisneyDining_TwoSupplements_MapTwoDisneyProducts()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6972415",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6972415").ToLowerInvariant(),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            var package = booking.Booking?.HolidayPackageDetails;
            Assert.NotNull(package);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            var disneyProducts = booking.Booking.HolidayProducts.Where(p => p.GenericProductDetails != null && p.GenericProductDetails.ElementGroup == Constants.Application.DisneyElementGroupCode).ToList();
            Assert.Equal(2, disneyProducts.Count);
            Assert.True(disneyProducts.All(x => x.IsDerivedFromHotelSupplement == true));
        }

        [Fact]
        public async Task RetrieveListBookingService_DisneyHotel_MapDisneyProducts()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var retrieveBookingRequests = new List<TripsDetailsRequest>()
            {
                new()
                    {
                           BookingReference = "6843310",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = "TEST",
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                new()
                    {
                           BookingReference = "6843319",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = "TEST",
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                new()
                    {
                           BookingReference = "6843314",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = "TEST",
                           Using = RequestOptionCodeType.HOLIDAYS

                    },
                new()
                    {
                           BookingReference = "6843312",
                           DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                           Surname = "TEST",
                           Using = RequestOptionCodeType.HOLIDAYS

                    }
            };

            var listResponse = await retrieveBookingService.HandleList(retrieveBookingRequests, CancellationToken.None);

            Assert.NotNull(listResponse);
            Assert.Equal(4, listResponse.TripListResponse.Where(rs => rs.TripDetails.Status == 200).Count());
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts != null));
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts
                .Where(x => x.TbxProductType == "GEN")
                .All(x => x.GenericProductDetails != null)));
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts.Any(x => x.TbxProductType == "GEN")));
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts
                .Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.GenericProductDetails.ElementGroup == Constants.Application.DisneyElementGroupCode)
                .All(x => x.Summary.Name.Contains("Disney"))));
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts
                .Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.GenericProductDetails.ElementGroup == Constants.Application.DisneyElementGroupCode)
                .All(x => x.Summary.Highlight.Contains("Disney"))));
            Assert.True(listResponse.TripListResponse.All(rs => rs.TripDetails.Booking.HolidayProducts
                .Where(x => x.IsAncillaryProduct == true && x.TbxProductType == "GEN" && x.GenericProductDetails.ElementGroup == Constants.Application.DisneyElementGroupCode)
                .All(x => x.IsDerivedFromHotelSupplement == true)));

        }

        [Fact]
        public async Task RetrieveBookingService_HotelDisney_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6834428",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Test",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            //assert holidayproducts have 3 disney products in it 
            Assert.Equal(3, booking.Booking.HolidayProducts.Count(p => p?.GenericProductDetails?.ElementGroup == Constants.Application.DisneyElementGroupCode));
            var hotelProduct = booking.Booking.HolidayProducts.FirstOrDefault(p => p.TbxProductType == "HTL");
            Assert.NotNull(hotelProduct);
            Assert.NotNull(hotelProduct.HotelDetails);
            Assert.Equal(1, hotelProduct.HotelDetails.NumberOfRooms);
        }

        [Fact]
        public async Task RetrieveBookingService_HotelRoomsCount_HolidayProduct()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6834428",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "Test",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.HolidayProducts);
            var hotelProduct = booking.Booking.HolidayProducts.FirstOrDefault(p => p.TbxProductType == "HTL");
            Assert.NotNull(hotelProduct);
            Assert.NotNull(hotelProduct.HotelDetails);
            Assert.Equal(1, hotelProduct.HotelDetails.NumberOfRooms);
        }

        [Fact]
        public async Task RetrieveBookingService_ReturnAllAdvisoryNotes()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6996245",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = GetLeadTravellerSurname("6996245"),
                Using = RequestOptionCodeType.HOLIDAYS

            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            Assert.NotNull(booking);
            Assert.NotNull(booking.Booking?.AdvisoryNotes);
            Assert.All(booking.Booking.AdvisoryNotes, note =>
            {
                Assert.False(string.IsNullOrWhiteSpace(note.Number));
                Assert.False(string.IsNullOrWhiteSpace(note.Code));
                Assert.False(string.IsNullOrWhiteSpace(note.Description));
                Assert.False(string.IsNullOrWhiteSpace(note.NoteText));
                Assert.False(string.IsNullOrWhiteSpace(note.Priority));
                Assert.False(string.IsNullOrWhiteSpace(note.Title));
                Assert.NotNull(note.Compulsory);
            });
        }

        [Fact]
        public async Task RetrieveBookingService_UnsupportedFlightsSegments_ReturnsPnrLocator()
        {
            var retrieveBookingService = _serviceProvider.GetService<IRetrieveBookingService>()!;
            var dob = DateOnly.TryParse("1982-09-14", out var parsedDob) ? parsedDob : default;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "6995709",
                DateOfBirth = dob,
                Surname = GetLeadTravellerSurname("6995709"),
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await retrieveBookingService.Handle(request, CancellationToken.None);

            var segments = booking.Booking.HolidayProducts.First(x => x.TbxProductType == "FLT").FlightDetails.Sectors.First().Segments;
            Assert.Equal("GULLBF", segments.First().PnrLocator);
            Assert.Equal("GULLBF", segments.Last().PnrLocator);
        }
    }
}


