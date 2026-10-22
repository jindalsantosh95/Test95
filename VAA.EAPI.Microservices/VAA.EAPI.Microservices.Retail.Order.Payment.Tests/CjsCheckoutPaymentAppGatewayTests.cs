using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CjsCheckout;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;
using PaymentEnvironment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    [Collection("CJS payment configuration")]
    public sealed class CjsCheckoutPaymentAppGatewayTests
    {
        private readonly IServiceProvider _serviceProvider;

        public CjsCheckoutPaymentAppGatewayTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<CjsCheckoutPaymentAppGateway>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task PostPaymentAppAsync_ReturnsOk_AndIncludesPaymentLink()
        {
            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAppUrl, "https://example.test/cjs/payment/app/test1");
            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var gateway = _serviceProvider.GetRequiredService<CjsCheckoutPaymentAppGateway>();

            var payload = new CjsCheckoutPaymentAppPayload("r", "p", "e", "c");

            var result = await gateway.StartPaymentSessionAsync(
                payload,               
                cookieHeader: "mmb-session=xyz",
                cancellationToken: CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Body);
            Assert.NotNull(result.Body!.PaymentSessions);
            Assert.False(string.IsNullOrWhiteSpace(result.Body.PaymentSessions![0].PaymentLink));
        }

        [Fact]
        public async Task CjsCheckoutPaymentAmount_ReturnsOk_AndIncludesPaymentLink()
        {
            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAmountUrl, "https://example.test/cjs/payment/app/test2");
            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var gateway = _serviceProvider.GetRequiredService<CjsCheckoutPaymentAppGateway>();

            var result = await gateway.SetPaymentAmountAsync(
                amount: 3900.00m,
                cookies: ["mmb-session=xyz", "com.virginholidays.payment.v2=orig"],
                cancellationToken: CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Body);
            Assert.NotNull(result.Body!.PaymentSessions);
            Assert.False(string.IsNullOrWhiteSpace(result.Body.PaymentSessions![0].PaymentLink));

            Assert.NotNull(result.SetCookieHeaders);
            Assert.Contains("mmb-session=xyz", result.SetCookieHeaders);
            Assert.Contains("com.virginholidays.payment.v2=orig", result.SetCookieHeaders);
        }

    }
}
