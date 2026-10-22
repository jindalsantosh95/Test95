using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.MmbQuery;
using Xunit;
using Environment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;
using PaymentErrorCodes = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.ErrorCodes;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    [Collection("CJS payment configuration")]
    public sealed class CjsCheckoutPaymentAppTests
    {
        private readonly IServiceProvider _serviceProvider;

        public CjsCheckoutPaymentAppTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<XunitHttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<XunitHttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMmbQueryLoginGateway, MmbQueryLoginGateway>()
                .AddSingleton<ICjsCheckoutPaymentAppGateway, CjsCheckoutPaymentAppGateway>()
                .AddSingleton<ICjsCheckoutPaymentApp, CjsCheckoutPaymentApp>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_ReturnsBadRequest_WhenBodyIsEmpty()
        {
            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(string.Empty, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.NotEmpty(result.Value.Errors);
        }

        [Fact]
        public async Task HandleAsync_ReturnsBadRequest_WhenBodyIsInvalidJson()
        {
            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync("{not-json", CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.NotEmpty(result.Value.Errors);
        }

        [Fact]
        public async Task HandleAsync_WithoutAmount_ReturnsOk_AndSurfacesDownstreamSetCookieHeaders()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(
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
                """,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("CPD", result.Value.Value?.Type);

            // Only allowed cookies are surfaced, and they are returned as multiple Set-Cookie headers (one per cookie-pair).
            Assert.Collection(
                result.Value.HeaderParameters,
               h =>
               {
                   Assert.Equal("Set-Cookie", h.Key);
                   Assert.Equal("mmb-session=xyz", h.Value);
               },
               h =>
               {
                   Assert.Equal("Set-Cookie", h.Key);
                   Assert.Equal("com.virginholidays.payment.v2=orig", h.Value);
               });
        }

        [Fact]
        public async Task HandleAsync_ReturnsBadRequest_WhenMandatoryFieldsMissing()
        {
            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(
                """
                {
                  "bookingReference": "",
                  "surname": "Smith",
                  "dateOfBirth": "1990-01-01",
                  "returnUrl": "r",
                  "payPalClientUrl": "p",
                  "expiryUrl": "e",
                  "contextPath": "c"
                }
                """,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == PaymentErrorCodes.HolidayPaymentValidationError);
        }

        [Fact]
        public async Task HandleAsync_ReturnsUnauthorized_WhenLoginReturnsUnauthorized()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login/unauthorized");

            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(ValidRequestBody, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Unauthorized, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == PaymentErrorCodes.HolidayPaymentLoginError);
        }

        [Fact]
        public async Task HandleAsync_ReturnsError_WhenDownstreamReturnsInternalServerError()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/error500");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(ValidRequestBody, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == PaymentErrorCodes.HolidayPaymentProcessingError);
        }

        [Fact]
        public async Task HandleAsync_ReturnsBadRequest_WhenAmounttoPayGreaterThanDownstreamAmount()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/amountgreater");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            string requestBody =
            """
            {
              "bookingReference": "ABC123",
              "surname": "Smith",
              "dateOfBirth": "1990-01-01",
              "returnUrl": "r",
              "payPalClientUrl": "p",
              "expiryUrl": "e",
              "contextPath": "c",
              "amountToPay": 4000.00
            }
            """;
            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(requestBody, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == PaymentErrorCodes.HolidayPaymentValidationError);
        }

        [Fact]
        public async Task HandleAsync_ReturnsBadRequest_WhenAmounttoPayIsZero()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAmountUrl, "https://example.test/cjs/payment/app/test2");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            string requestBody =
            """
            {
              "bookingReference": "ABC123",
              "surname": "Smith",
              "dateOfBirth": "1990-01-01",
              "returnUrl": "r",
              "payPalClientUrl": "p",
              "expiryUrl": "e",
              "contextPath": "c",
              "amountToPay": 0
            }
            """;
            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(requestBody, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == PaymentErrorCodes.HolidayPaymentValidationError);
        }

        [Fact]
        public async Task HandleAsync_ReturnsFailure_WhenDownstreamReturnsOkButBodyIsNull()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/ok-empty");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(ValidRequestBody, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Contains(PaymentErrorCodes.HolidayPaymentProcessingError, result.Error);
        }

        private const string ValidRequestBody =
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


        [Fact]
        public async Task HandleAsync_WithAmount_ReturnsOk_AndSurfacesDownstreamSetCookieHeaders()
        {
            ConfigReader.Set(Environment.MmbQueryLoginUrl, "https://mmb-login.test/login");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAmountUrl, "https://example.test/cjs/payment/app/test2");
            ConfigReader.Set(Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var app = _serviceProvider.GetRequiredService<ICjsCheckoutPaymentApp>();

            var result = await app.HandleAsync(
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
                """,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("CPD", result.Value.Value?.Type);

            // Only allowed cookies are surfaced, and they are returned as multiple Set-Cookie headers (one per cookie-pair).
            Assert.Collection(
                result.Value.HeaderParameters,
                h =>
                {
                    Assert.Equal("Set-Cookie", h.Key);
                    Assert.Equal("mmb-session=xyz", h.Value);
                },
                h =>
                {
                    Assert.Equal("Set-Cookie", h.Key);
                    Assert.Equal("com.virginholidays.payment.v2=orig", h.Value);
                });
        }
    }
}
