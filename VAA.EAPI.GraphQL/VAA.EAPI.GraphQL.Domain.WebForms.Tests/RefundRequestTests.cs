using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Caching;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.WebForms.Application;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Services;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Services;
using VAA.EAPI.GraphQL.Domain.WebForms.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;
using VAA.EAPI.GraphQL.Infrastructure.Caching;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.AzureBlob;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Tests
{
    public class RefundRequestTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RefundRequestTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("CRM:Refunds:Status:Url", "https://digital-test.virginatlantic.com/dev/Refunds/Status");
            ConfigReader.Set("CRM:Refunds:Submit:Url", "https://digital-test.virginatlantic.com/dev/Refunds/Submit");
            ConfigReader.Set("CRM:Refunds:AppId", "stuff");
            ConfigReader.Set("CRM:Refunds:OcpKey", "things");


            ConfigReader.Set("OrderData:MSUrl", "https://Testresources.virginatlantic.com/sit/orderdata");
            ConfigReader.Set("OrderData:AppId", "https://Testresources.virginatlantic.com/sit/orderdata");
            ConfigReader.Set("OrderData:OcpSubID", "https://Testresources.virginatlantic.com/sit/orderdata");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                      .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<IBookingProvider, BookingProvider>()
                                                      .AddSingleton<IRefundProvider, RefundProvider>()
                                                      .AddSingleton<IRefundRequestService, RefundRequestService>()
                                                      .BuildServiceProvider();

            //AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }


        // TODO: FindBooking is not yet implemented.

        /*
        [Fact]
        public async Task FindMyBookingTests()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IRefundRequestService>();
            var form = new RefundForm()
            {
                Requester = new RefundRequester
                {
                    FirstName = "John",
                    LastName = "Doe",
                    BookingReference = "ABC123",
                    Email = "john.doe@example.com",
                    BookingDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
                }
            };

            // Act
            var response = await handler.FindMyBooking(form);

            Assert.Equal(4, response.Entries.Count());  // Mocked response contains 4 entries
            Assert.All(response.Entries.Where(x => x.FlightNumber == 1), item => Assert.False(string.IsNullOrWhiteSpace(item.ClaimNumber))); // Entries with FlightNumber 1 should have a ClaimNumber
            Assert.All(response.Entries.Where(x => x.FlightNumber == 2), item => Assert.True(string.IsNullOrWhiteSpace(item.ClaimNumber))); // Entries with FlightNumber 2 should not have a ClaimNumber

        }
        */

        [Fact]
        public async Task SubmitRefundRequestTests()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IRefundRequestService>();
            var form = new RefundForm()
            {
                Requester = new RefundRequester
                {
                    FirstName = "John",
                    LastName = "Doe",
                    BookingReference = "ABC123",
                    Email = "john.doe@example.com",
                    BookingDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
                },
                Entries = new List<RefundItem>
                {
                    new RefundItem
                    {
                        PassengerRef = "01.01",
                        PassengerName = "John Doe",
                        FlightDate = DateOnly.Parse("2026-03-01"),
                        FlightNumber = 2,
                        TicketNumber = "9321234567890",
                        CouponNumber = "2"
                    },
                    new RefundItem
                    {
                        PassengerRef = "02.01",
                        PassengerName = "Jane Doe",
                        FlightDate = DateOnly.Parse("2026-03-01"),
                        FlightNumber = 2,
                        TicketNumber = "9321234567891",
                        CouponNumber = "2",
                    }
                }
            };

            // Act
            var response = await handler.SubmitRefundRequest(form);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("CLAIM123", response);

            // TODO: Negative test cases (claim already exists, failure response from CRM, etc.) - Need to enhance the mock to simulate these scenarios
        }
    }
}
