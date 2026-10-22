using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Retailing.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Tests
{
    public class BookingsQueriesTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingsQueriesTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("OrderDataProvider:MsUrl", "https://tempuri.org/orderdata");
            ConfigReader.Set("CateringProvider:CateringMsUri", "https://tempuri.org/catering");
            ConfigReader.Set("OrderSearchProvider:OrderSearchMsUri", "https://tempuri.org");
            ConfigReader.Set("OrderSearchProvider:OcpKey", "12345678");
            ConfigReader.Set("cateringprovider:OcpKey", "12345678");
            ConfigReader.Set("orderdataprovider:OcpKey", "12345678");
            ConfigReader.Set("retailing:channelid", "testChannel");
        

        _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<ICateringProvider, CateringProvider>()
                                                      .AddSingleton<IOrderSearchProvider, OrderSearchProvider>()
                                                      .AddSingleton<IOrderDataProvider, OrderDataProvider>()
                                                      .AddSingleton<BookingsQueries>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };

            var tokenString = TestDataProvider.GetTestData<string>("Account//LoginTokenOtherToken");

            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), Json.Deserialize<AuthToken>(tokenString)));
        }

        [Fact]
        public async Task Execute_BookingsQueries_RetrieveBooking()
        {
            var bookingsQueries = _serviceProvider.GetService<BookingsQueries>()!;

            string bookingRef = "PNR1234";
            string firstName = "bob";
            string lastName = "smith";

            var result = await bookingsQueries.RetrieveBooking(bookingRef, firstName, lastName);

            Assert.NotNull(result);
            Assert.Equal("2025-10-10".Parse<DateTime>(), result.BookingDate);
            Assert.Equal(bookingRef, result.RecordLocator);
            Assert.Single(result.Passengers!);
            Assert.Single(result.Passengers!, x => x.FirstName.Is(firstName) && x.LastName.Is(lastName));
        }

        [Fact]
        public async Task Execute_BookingsQueries_RetrieveBookings()
        {
            var bookingsQueries = _serviceProvider.GetService<BookingsQueries>()!;
            var jwtSecurityToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()!
                                                                        .AccessToken
                                                                        .OtherToken
                                                                        .ToJwtSecurityToken();

            var fcNumber = jwtSecurityToken.Claims.First(x => x.Type.Is("flyingclubNumber")).Value;

            var result = await bookingsQueries.RetrieveBookings();

            Assert.NotNull(result);
            Assert.Equal(fcNumber, result.FCNumber);
            Assert.Single(result.Bookings!);
        }
    }
}