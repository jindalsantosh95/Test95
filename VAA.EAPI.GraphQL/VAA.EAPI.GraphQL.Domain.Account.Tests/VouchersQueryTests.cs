using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class VouchersQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public VouchersQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var jwtFeature = new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = "dummy-token" });
            AsyncContext.Current.Context.Features.Set(jwtFeature);

            var tokenProvider = new TestOAuthTokenProvider();

            var services = new ServiceCollection()
               .AddLogging()
               .AddSingleton<IOAuthTokenProvider>(tokenProvider)
               .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
               .AddSingleton<IHttpConnector, VAA.EAPI.GraphQL.Infrastructure.Services.HttpConnector>()
               .AddSingleton<IAccountCustomerService, CustomerService>()
               .AddSingleton(typeof(IRefDataService<>), typeof(RefDataService<>))
               .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
               .AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
               .AddSingleton<IVoucherQueryProvider, VoucherQueryProvider>()
               .AddSingleton<VouchersQuery>()
               .BuildServiceProvider();

            _serviceProvider = services;
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            var tokenB2CProvider = _serviceProvider.GetRequiredService<VAA.EAPI.GraphQL.Infrastructure.Interfaces.ITokenB2CProvider<string>>();
            var customerProvider = _serviceProvider.GetRequiredService<IAccountCustomerService>();

            Assert.Throws<ArgumentNullException>(() => new VouchersQuery(null, tokenB2CProvider, customerProvider));

        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenTokenProviderIsNull()
        {
            var customerProvider = _serviceProvider.GetRequiredService<IAccountCustomerService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<VouchersQuery>>();

            Assert.Throws<ArgumentNullException>(() => new VouchersQuery(logger, null, customerProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenCustomerProviderIsNull()
        {
            var tokenB2CProvider = _serviceProvider.GetRequiredService<VAA.EAPI.GraphQL.Infrastructure.Interfaces.ITokenB2CProvider<string>>();
            var logger = _serviceProvider.GetRequiredService<ILogger<VouchersQuery>>();

            Assert.Throws<ArgumentNullException>(() => new VouchersQuery(logger, tokenB2CProvider, null));
        }

        [Fact]
        public async Task GetVoucherDetails_ShouldThrowArgumentNullException_WhenAccessTokenIsMissing()
        {
            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<VouchersQuery>();

            SetAsyncContext(string.Empty);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => query.GetVoucherDetails(voucherQueryProvider, 1, 10));

            Assert.Equal("Authorization", exception.ParamName);
        }

        [Fact]
        public async Task GetVoucherDetails_ShouldReturnVoucherDetails_WhenCustomerIdExistsInToken()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");

            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<VouchersQuery>();

            SetAsyncContext("valid-token");

            var result = await query.GetVoucherDetails(voucherQueryProvider, 1, 20);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("Gold Upgrade Reward", result.First().Type);
            Assert.Equal("Virgin Money", result.First().Source);
        }

        [Fact]
        public async Task GetVoucherDetails_ShouldResolveCustomerId_WhenMissingInToken()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/CustomerIdMissing");

            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<VouchersQuery>();

            SetAsyncContext("valid-token");

            var result = await query.GetVoucherDetails(voucherQueryProvider, 1, 20);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("Gold Upgrade Reward", result.First().Type);
            Assert.Equal("Virgin Money", result.First().Source);
        }

        private static void SetAsyncContext(string token)
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var jwtFeature = new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = token });
            AsyncContext.Current.Context.Features.Set(jwtFeature);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}