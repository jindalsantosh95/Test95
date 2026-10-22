using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnerDataManagementQueryProviderTests
    {
        public PartnerDataManagementQueryProviderTests()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.PdmMSUrl, "https://testurl.api.com/pdm");
            ConfigReader.Set(Environment.PdmMSAppId, "pdm-app-id");
            ConfigReader.Set(Environment.CommonEAPIOcpAPIMKey, "pdm-ocp-key");
        }

        [Fact]
        public async Task GetVSRetroFlownData_ShouldUseMockFile_AndReturnVsRetroFlownDetails()
        {
            var httpClientFactory = new HttpClientFactory();

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<System.Net.Http.IHttpClientFactory>(httpClientFactory)
                .AddSingleton<ILogger<HttpConnector>>(NullLogger<HttpConnector>.Instance)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(new FixedOAuthTokenProvider("abc"))
                .AddSingleton<PartnerDataManagementQueryProvider>()
                .BuildServiceProvider();

            var provider = serviceProvider.GetRequiredService<PartnerDataManagementQueryProvider>();

            var result = await provider.GetVSRetroFlownData(new GetVSRetroFlownDataRequest
            {
                TicketNumber = "9392008105719"
            });

            Assert.IsType<VSFlownDetailsResponse>(result);
            var detail = Assert.Single(result.VSRetroFlownDetails);
            Assert.Equal(24223788, detail.Id);
            Assert.Equal("9392008105719", detail.TicketNumber);
            Assert.Equal("KK2YY3", detail.RecordLocatorOperatingCarrier);

            var responses = httpClientFactory.GetResponses("https://testurl.api.com/pdm");
            Assert.NotEmpty(responses);
        }

        [Fact]
        public async Task ValidateVSClaimInputs_ShouldUseMockFile_AndReturnValidationResponse()
        {
            var httpClientFactory = new HttpClientFactory();

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<System.Net.Http.IHttpClientFactory>(httpClientFactory)
                .AddSingleton<ILogger<HttpConnector>>(NullLogger<HttpConnector>.Instance)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(new FixedOAuthTokenProvider("abc"))
                .AddSingleton<PartnerDataManagementQueryProvider>()
                .BuildServiceProvider();

            var provider = serviceProvider.GetRequiredService<PartnerDataManagementQueryProvider>();

            var result = await provider.ValidateVSClaimInputs(new ValidateVSClaimInputsRequest
            {
                FareBasis = "Y26",
                TicketDesignator = "ITF",
                PassengerType = "ITF"
            });

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.NotEmpty(httpClientFactory.GetResponses("https://testurl.api.com/pdm"));
        }

        [Fact]
        public async Task UpdateVSRetroFlownDetails_ShouldUseMockFile_AndReturnResponse()
        {
            var httpClientFactory = new HttpClientFactory();

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<System.Net.Http.IHttpClientFactory>(httpClientFactory)
                .AddSingleton<ILogger<HttpConnector>>(NullLogger<HttpConnector>.Instance)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(new FixedOAuthTokenProvider("abc"))
                .AddSingleton<PartnerDataManagementQueryProvider>()
                .BuildServiceProvider();

            var provider = serviceProvider.GetRequiredService<PartnerDataManagementQueryProvider>();

            var result = await provider.UpdateVSRetroFlownDetails(new EditVSRetroFlownDataRequest
            {
                OriginAirport = "LHR",
                DestinationAirport = "JFK",
                TicketNumber = "9321234567890",
                Status = "Allocated",
                VALoyaltyProgramCode = "VS",
                MemberNumber = "1234567890"
            });

            Assert.Null(result.ErrorMessage);
            Assert.Equal("VS retro flown details updated successfully.", result.SuccessMessage);
            Assert.NotEmpty(httpClientFactory.GetResponses("https://testurl.api.com/pdm"));
        }

        private class FixedOAuthTokenProvider(string token) : IOAuthTokenProvider
        {
            private readonly string _token = token;

            public Task<string> AcquireTokenAsync(string appId) => Task.FromResult(_token);
        }
    }
}
