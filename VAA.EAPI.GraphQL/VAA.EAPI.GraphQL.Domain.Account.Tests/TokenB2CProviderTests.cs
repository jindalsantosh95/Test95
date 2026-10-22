using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class TokenB2CProviderTests
    {
        private readonly HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public TokenB2CProviderTests()
        {
            _httpClientFactory = new HttpClientFactory();
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(_httpClientFactory)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IAccountCustomerService, CustomerService>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldNotCallCustomer_WhenIdentityReturnsFcNumberAndMemberStatus()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");
            ConfigReader.Set("Customer:MsUrl", "https://Testresources.virginatlantic.com/dev/microservices/entra-signup-error");

            var connector = _serviceProvider.GetRequiredService<IHttpConnector>();
            var tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
            var customerService = _serviceProvider.GetRequiredService<IAccountCustomerService>();
            var provider = new TokenB2CProvider<string>(tokenProvider, connector, customerService, Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);

            var customerLookupPath = "/dev/microservices/entra-signup-error/customerProfileSearch";
            var customerCallsBefore = _httpClientFactory
                .GetResponses("https://Testresources.virginatlantic.com")
                .Count(response => response.RequestMessage?.RequestUri?.AbsolutePath.Contains(customerLookupPath, StringComparison.OrdinalIgnoreCase) == true);

            var response = await provider.ValidateTokenAsync("jwt-token");
            var claim = Json.Deserialize<TokenB2CResponse>(response);

            var customerCallsAfter = _httpClientFactory
                .GetResponses("https://Testresources.virginatlantic.com")
                .Count(response => response.RequestMessage?.RequestUri?.AbsolutePath.Contains(customerLookupPath, StringComparison.OrdinalIgnoreCase) == true);

            Assert.Equal("1074840875", claim.FcNumber);
            Assert.Equal("Active", claim.MemberStatus);
            Assert.Equal(customerCallsBefore, customerCallsAfter);
        }

        [Fact]
        public async Task ValidateTokenAsync_ShouldFillMissingMemberStatusFromCustomerProfileSearch()
        {
            ConfigReader.Set("Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/MemberStatusMissing");
            ConfigReader.Set("Customer:MsUrl", "https://Testresources.virginatlantic.com/dev/microservices");

            var connector = _serviceProvider.GetRequiredService<IHttpConnector>();
            var tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
            var customerService = _serviceProvider.GetRequiredService<IAccountCustomerService>();
            var provider = new TokenB2CProvider<string>(tokenProvider, connector, customerService, Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);

            var response = await provider.ValidateTokenAsync("jwt-token");
            var claim = Json.Deserialize<TokenB2CResponse>(response);

            Assert.Equal("identity-customer-id", claim.CustomerId);
            Assert.Equal("9106430888", claim.FcNumber);
            Assert.Equal("Active", claim.MemberStatus);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource)
            {
                _ = resource;
                return Task.FromResult("access-token");
            }
        }

    }
}
