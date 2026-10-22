using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public sealed class HolidayPaymentMutationTests 
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HolidayPaymentMutation _mutation;

        public HolidayPaymentMutationTests()
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
                                                      .AddSingleton<HolidayPaymentMutation>()
                                                      .BuildServiceProvider();

            _mutation = _serviceProvider.GetRequiredService<HolidayPaymentMutation>();
        }

        [Fact]
        public async Task HolidayPaymentMutation_NullRequest_ThrowsArgumentNullException()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() => _mutation.HolidayPayment(null));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task HolidayPaymentMutation_NullAmountToPay_ReturnsPaymentSession()
        {
            var tripResourceId = await BookingIdHelper.CreateBookingId(
                existingBookingId: null,
                msBookingId: "booking-123",
                givenName: null,
                surName: "Doe",
                pnrLocator: null,
                ticketNumber: null,
                dob: "1990-01-01");

            var response = await _mutation.HolidayPayment(new HolidayPaymentRequest
            {
                TripResourceId = tripResourceId,
                AmountToPay = null,
                ReturnUrl = "r",
                PayPalClientUrl = "p",
                ExpiryUrl = "e",
                ContextPath = "c"
            });

            Assert.NotNull(response);
            Assert.Equal(2758.65m, response.AmountToPay);
            Assert.Equal("PENDING", response.Status);
            Assert.Equal("https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment/hpp?id=E0f_TGuAYxjDZ-WuF6bT8Q", response.PaymentLink);
            Assert.Equal("https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment", response.RemoveLink);          
        }

        [Fact]
        public async Task HolidayPaymentMutation_Response_ContainsExpectedCookies()
        {
            var tripResourceId = await BookingIdHelper.CreateBookingId(
                existingBookingId: null,
                msBookingId: "booking-123",
                givenName: null,
                surName: "Doe",
                pnrLocator: null,
                ticketNumber: null,
                dob: "1990-01-01");

            var response = await _mutation.HolidayPayment(new HolidayPaymentRequest
            {
                TripResourceId = tripResourceId,
                AmountToPay = null,
                ReturnUrl = "r",
                PayPalClientUrl = "p",
                ExpiryUrl = "e",
                ContextPath = "c"
            });

           
            Assert.NotNull(response);

            Assert.True(AsyncContext.Current.Context.Items.TryGetValue(Common.Constants.Application.ResponseHeaderKey, out var headersObject));
            var headers = Assert.IsAssignableFrom<IDictionary<string, string>>(headersObject);
            Assert.True(headers.TryGetValue("Set-Cookie", out var setCookieValue));
            Assert.NotNull(setCookieValue);

            var setCookies = setCookieValue.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.True(setCookies.Length >= 2);
            Assert.Contains(setCookies, v => v.StartsWith("mmb-session=", StringComparison.Ordinal) && v.Length > "mmb-session=".Length);
            Assert.Contains(setCookies, v => v.StartsWith("com.virginholidays.payment.v2=", StringComparison.Ordinal) && v.Length > "com.virginholidays.payment.v2=".Length);
        }

        [Fact]
        public async Task HolidayPaymentMutation_NegativeAmountToPay_ThrowsArgumentOutOfRangeException()
        {
            var tripResourceId = await BookingIdHelper.CreateBookingId(
                existingBookingId: null,
                msBookingId: "booking-123",
                givenName: null,
                surName: "Doe",
                pnrLocator: null,
                ticketNumber: null,
                dob: "1990-01-01");

            var ex = await Assert.ThrowsAsync<Exception>(() => _mutation.HolidayPayment(new HolidayPaymentRequest
            {
                TripResourceId = tripResourceId,
                AmountToPay = -1m,
                ReturnUrl = "r",
                PayPalClientUrl = "p",
                ExpiryUrl = "e",
                ContextPath = "c"
            }));

            Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        }


    }
}
