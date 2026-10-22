using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.MmbQuery;
using VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http;
using Xunit;
using Environment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    [Collection("CJS payment configuration")]
    public sealed class CjsPaymentControllerTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TestFunctionContext _functionContext;

        public CjsPaymentControllerTests()
        {
            // Current directory is set once for the test assembly in TestEnvironment.Initialize().
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMmbQueryLoginGateway, MmbQueryLoginGateway>()
                .AddSingleton<ICjsCheckoutPaymentAppGateway, CjsCheckoutPaymentAppGateway>()
                .AddSingleton<ICjsCheckoutPaymentApp, CjsCheckoutPaymentApp>()
                .AddSingleton<CjsPaymentController>()
                .BuildServiceProvider();

            _functionContext = new TestFunctionContext
            {
                InstanceServices = _serviceProvider
            };

            AsyncContext.Current = new AsyncContext { Context = _functionContext };
        }

        [Fact]
        public async Task PaymentApp_ReturnsOk_AndBindsSetCookieHeaders()
        {
            // Environment/config values aligned to the request URLs in the .mock files.
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://example.test/mmb-query-api/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var controller = _serviceProvider.GetRequiredService<CjsPaymentController>();

            var requestJson =
                """
                {
                  "bookingReference": "ABC123",
                  "surname": "Smith",
                  "dateOfBirth": "1990-01-01",
                  "returnUrl": "r",
                  "payPalClientUrl": "p",
                  "expiryUrl": "e",
                  "contextPath": "c"
                }
                """;

            using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
            var request = new TestHttpRequestData(
                _functionContext,
                new Uri("http://localhost/holiday/payment"),
                bodyStream,
                method: "POST");

            var response = await controller.PaymentApp(request, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieValues));
            var cookies = (setCookieValues ?? Array.Empty<string>()).ToArray();
            Assert.Contains("mmb-session=xyz", cookies);
            Assert.Contains("com.virginholidays.payment.v2=orig", cookies);
        }

        [Fact]
        public async Task PaymentApp_ReturnsBadRequest_WhenBodyIsEmpty()
        {
            var controller = _serviceProvider.GetRequiredService<CjsPaymentController>();

            using var bodyStream = new MemoryStream(Array.Empty<byte>());
            var request = new TestHttpRequestData(
                _functionContext,
                new Uri("http://localhost/holiday/payment"),
                bodyStream,
                method: "POST");

            var response = await controller.PaymentApp(request, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PaymentApp_WithAmount_ReturnsOk_AndBindsSetCookieHeaders()
        {
            // Environment/config values aligned to the request URLs in the .mock files.
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://example.test/mmb-query-api/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAmountUrl, "https://example.test/cjs/payment/app/test2");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var controller = _serviceProvider.GetRequiredService<CjsPaymentController>();

            var requestJson =
                """
                {
                  "bookingReference": "ABC123",
                  "surname": "Smith",
                  "dateOfBirth": "1990-01-01",
                  "returnUrl": "r",
                  "payPalClientUrl": "p",
                  "expiryUrl": "e",
                  "contextPath": "c",
                  "amountToPay": 3900.00
                }
                """;

            using var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
            var request = new TestHttpRequestData(
                _functionContext,
                new Uri("http://localhost/holiday/payment"),
                bodyStream,
                method: "POST");

            var response = await controller.PaymentApp(request, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieValues));
            var cookies = (setCookieValues ?? Array.Empty<string>()).ToArray();
            Assert.Contains("mmb-session=xyz", cookies);
            Assert.Contains("com.virginholidays.payment.v2=orig", cookies);
        }
    }
}

