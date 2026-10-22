using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services
{
    public class HolidayProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public HolidayProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:Url", "https://testresources.virginatlantic.com/dev/digital/holidays");
            ConfigReader.Set("TripSettings:HolidayProvider:HolidaysMs:OcpSubID", Guid.NewGuid().ToString());

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IHolidayProvider, HolidayProvider>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task RetrieveBooking()
        {
            var holidayProvider = _serviceProvider.GetService<IHolidayProvider>()!;
            var request = new TripsDetailsRequest()
            {
                BookingReference = "1234567890",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "TEST",
                Using = RequestOptionCodeType.HOLIDAYS
            };

            var booking = await holidayProvider.RetrieveBooking(request);

            Assert.NotNull(booking);
        }

        [Fact]
        public async Task RetrieveBookingList()
        {
            var holidayProvider = _serviceProvider.GetService<IHolidayProvider>()!;
            var holidayRequestList = new List<TripsDetailsRequest>()
            {
                new()
                {
                    BookingReference = "111111",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = "TEST",
                    Using = RequestOptionCodeType.HOLIDAYS
                },
                new()
                {
                    BookingReference = "6666666",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                    Surname = "TEST",
                    Using = RequestOptionCodeType.HOLIDAYS
                }
            };

            var bookingList = await holidayProvider.RetrieveBookingList(holidayRequestList);

            Assert.NotNull(bookingList);

            Assert.Equal(2, bookingList.TripListResponse.Count());
            Assert.Equal("6666666", bookingList.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "6666666").TripDetails.Booking.Id);
            Assert.Equal(HttpStatusCode.OK, bookingList.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "6666666").TripDetails.Status);

            Assert.Null(bookingList.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "111111").TripDetails.Booking);
            Assert.Equal(HttpStatusCode.BadRequest, bookingList.TripListResponse.First(x => x.TripSearchRequest.BookingReference == "111111").TripDetails.Status);
        }
    }
}
