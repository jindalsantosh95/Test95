using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Http;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.MmbQuery;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;
using PaymentEnvironment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    [Collection("CJS payment configuration")]
    public sealed class MmbQueryLoginGatewayTests
    {
        private readonly IServiceProvider _serviceProvider;

        public MmbQueryLoginGatewayTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<MmbQueryLoginGateway>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_AndIncludesRefreshToken()
        {
            ConfigReader.Set(PaymentEnvironment.MmbQueryLoginUrl, "https://example.test/mmb-query-api/login");

            var gateway = _serviceProvider.GetRequiredService<MmbQueryLoginGateway>();

            var result = await gateway.LoginAsync(
                new MmbQueryLoginRequest("ABC123", "Smith", "1990-01-01"),
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Body);
            Assert.Equal("test-refresh-token", result.Body!.RefreshToken);
            Assert.Null(result.CookieHeader);
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_AndSurfacesCookieHeader_WhenSetCookiePresent()
        {
            ConfigReader.Set(PaymentEnvironment.MmbQueryLoginUrl, "https://mmb-login.test/login");

            var gateway = _serviceProvider.GetRequiredService<MmbQueryLoginGateway>();

            var result = await gateway.LoginAsync(
                new MmbQueryLoginRequest("ABC123", "Smith", "1990-01-01"),
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Body);
            Assert.Equal("test-refresh-token", result.Body!.RefreshToken);
            Assert.Equal("mmb-session=MjYwY2YzOTEtZjY1NC00NjZjLTgxZGQtOTc0N2Q0ZGY4ZjIw", result.CookieHeader);
        }

    }
}
