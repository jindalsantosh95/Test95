using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class UpdatePreferenceMutationTests
    {
        private readonly IServiceProvider _serviceProvider;


        public UpdatePreferenceMutationTests()
        {
            Setup.CreateTestEnvironmentSettings();

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var testContext = new TestFunctionContext();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "fake-user-id"),
                new(ClaimTypes.Name, "Hollis Denissee")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            testContext.Features.Set(new JwtPrincipalFeature(principal, JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));
            AsyncContext.Current = new AsyncContext() { Context = testContext };

            var tokenProvider = new Moq.Mock<IOAuthTokenProvider>();
            tokenProvider.Setup(m => m.AcquireTokenAsync(Moq.It.IsAny<string>())).ReturnsAsync("test-app-token");

            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
                .AddLogging()
                .AddSingleton<IOAuthTokenProvider>(tokenProvider.Object)
                .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, VAA.EAPI.GraphQL.Infrastructure.Services.HttpConnector>()
                .AddSingleton<VAA.EAPI.GraphQL.Domain.Account.Interface.IAccountCustomerService, CustomerService>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton<UpdatePreferenceMutation>()
                .BuildServiceProvider();

            _serviceProvider = services;
        }

        [Fact]
        public async Task UpdatePreference_ShouldThrow_WhenCustomerTransactionsServiceReturnsError()
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.CustomerMSURLForMemberDetails, "https://Testresources.virginatlantic.com/dev/microservices/customer/loyalty/{memberNumber}");
            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://Testresources.virginatlantic.com/sit/customertransactions/error");
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");
            var sut = _serviceProvider.GetRequiredService<UpdatePreferenceMutation>();
            var request = new UpdatePreferenceRequest
            {
                Preferences = [new AccountPreference
                {
                    PreferenceType = AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP,
                    PreferenceValue = ""
                }]
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GraphQLException>(() => sut.UpdatePreference(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("10200", error.Code);
        }

        [Fact]
        public async Task UpdatePreference_ShouldReturnTrue_WhenPreferencesAreValid()
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.CustomerMSURLForMemberDetails, "https://Testresources.virginatlantic.com/dev/microservices/customer/loyalty/{memberNumber}");
            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://Testresources.virginatlantic.com/sit/customertransactions/");
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var sut = _serviceProvider.GetRequiredService<UpdatePreferenceMutation>();
            var request = new UpdatePreferenceRequest
            {
                Preferences = [
                    new AccountPreference
                    {
                        PreferenceType = AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP,
                        PreferenceValue = "AB"
                    },
                    new AccountPreference
                    {
                        PreferenceType = AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions,
                        PreferenceValue = "Y"
                    }
                ]
            };

            // Act
            var result = await sut.UpdatePreference(request);

            // Assert
            Assert.True(result);
            var reqs = httpClientFactory.GetResponses(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)).First(x => x.RequestMessage!.RequestUri!.ToString().Is("https://Testresources.virginatlantic.com/sit/customertransactions/UpdatePreferences")).RequestMessage;
            var updatePrefReq = Json.Deserialize<UpdatePreference>(await reqs.Content!.ReadAsStringAsync());

            Assert.Contains(updatePrefReq.Preference, p => p.PreferenceType == AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP.ToString() && p.PreferenceValue == "AB");
            Assert.Contains(updatePrefReq.Preference, p => p.PreferenceType == AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions.ToString() && p.PreferenceValue == "Y");
        }
    }
}
