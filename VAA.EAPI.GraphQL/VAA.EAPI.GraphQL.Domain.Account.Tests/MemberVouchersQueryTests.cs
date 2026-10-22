using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Constants;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class MemberVouchersQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public MemberVouchersQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
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
               .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
               .AddSingleton(typeof(IRefDataService<>), typeof(RefDataService<>))
               .AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
               .AddSingleton<IVoucherQueryProvider, VoucherQueryProvider>()
               .AddSingleton<MemberVouchersQuery>()
               .BuildServiceProvider();

            _serviceProvider = services;
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();
            var customerProvider = _serviceProvider.GetRequiredService<IAccountCustomerService>();

            Assert.Throws<ArgumentNullException>(() => new MemberVouchersQuery(null, tokenB2CProvider, customerProvider));

        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenTokenProviderIsNull()
        {
            var customerProvider = _serviceProvider.GetRequiredService<IAccountCustomerService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<MemberVouchersQuery>>();

            Assert.Throws<ArgumentNullException>(() => new MemberVouchersQuery(logger, null, customerProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenCustomerProviderIsNull()
        {
            var tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();
            var logger = _serviceProvider.GetRequiredService<ILogger<MemberVouchersQuery>>();

            Assert.Throws<ArgumentNullException>(() => new MemberVouchersQuery(logger, tokenB2CProvider, null));
        }

        [Fact]
        public async Task GetMemberVoucherDetails_ShouldThrowArgumentNullException_WhenAccessTokenIsMissing()
        {
            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<MemberVouchersQuery>();

            SetAsyncContext(string.Empty);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => query.GetMemberVoucherDetails(voucherQueryProvider, 1, 10));

            Assert.Equal("Authorization", exception.ParamName);
        }

        [Fact]
        public async Task GetMemberVoucherDetails_ShouldReturnVoucherDetails_WhenCustomerIdExistsInToken()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");

            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<MemberVouchersQuery>();

            SetAsyncContext("valid-token");

            var result = await query.GetMemberVoucherDetails(voucherQueryProvider, 1, 2);

            Assert.NotNull(result);
            Assert.NotNull(result.Vouchers);
            Assert.Equal(2, result.Vouchers.Count());
            Assert.Equal(5, result.MemberTotalVoucherCount);
            Assert.NotNull(result.Categories);
            Assert.NotEmpty(result.Categories);
            Assert.Equal(VoucherCategory.REWARD, result.Categories.First().Code);
        }

        [Fact]
        public async Task GetMemberVoucherDetails_ShouldResolveCustomerId_WhenMissingInToken()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/CustomerIdMissing");

            var voucherQueryProvider = _serviceProvider.GetRequiredService<IVoucherQueryProvider>();
            var query = _serviceProvider.GetRequiredService<MemberVouchersQuery>();

            SetAsyncContext("valid-token");

            var result = await query.GetMemberVoucherDetails(voucherQueryProvider, 1, 2);

            Assert.NotNull(result);
            Assert.NotNull(result.Vouchers);
            Assert.Equal(2, result.Vouchers.Count());
            Assert.Equal(5, result.MemberTotalVoucherCount);
            Assert.NotNull(result.Categories);
            Assert.NotEmpty(result.Categories);
            Assert.Equal(VoucherCategory.REWARD, result.Categories.First().Code);
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