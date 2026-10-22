using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class EnrolmentProviderIntegrationTests : IClassFixture<HttpClientFactoryFixture>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClientFactory _httpClientFactory;
        private readonly TestStampingPNRsHandler _stampingHandler;

        public EnrolmentProviderIntegrationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            if (httpClientFactoryFixture == null) throw new ArgumentNullException(nameof(httpClientFactoryFixture));

            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set("EnrolmentTokenFlag", "false");
            ConfigReader.Set(Environment.AllowMemberEnrolToAzureB2C, "false");
            AsyncContext.Current = null;
            _stampingHandler = new TestStampingPNRsHandler();
            _httpClientFactory = httpClientFactoryFixture.Current;

            var endpoint = ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.DeDupFCGenUrl);
            var requestMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);

            httpClientFactoryFixture.PrepareHeaders(requestMsg, headers: null, bearerToken: "dummy-token", preventFollow202Location: false);

            var signupResponse = new FlyingClubSignUpResponse
            {
                MemberDetails = new MemberDetails { MemberNumber = "1501033656", Status = "Active" }
            };

            var responseContent = JsonConvert.SerializeObject(signupResponse);
            var responseMsg = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };
            responseMsg.RequestMessage = requestMsg;

            httpClientFactoryFixture.RegisterRequestResponse(requestMsg, responseMsg);

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector>(_ => new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current))
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IStampingPNRsHandler>(_stampingHandler)
                .AddSingleton<IEnrolmentProvider, EnrolmentProvider>()
                .BuildServiceProvider();
        }

        private static CreateCustomerAccountRequest BuildRequest(IEnumerable<AccountPreference> prefs)
        {
            return new CreateCustomerAccountRequest
            {
                CustomerDetails = new CustomerAccountDetails
                {
                    FirstName = "John",
                    LastName = "Doe",
                    ContactDetails = new AccountContactDetails
                    {
                        EmailDetails = new List<AccountEmailDetail>
                        {
                            new AccountEmailDetail { EmailId = "john.doe@example.com", PreferredFlag = "Y" }
                        }
                    },
                    Preferences = prefs == null ? null : new List<AccountPreference>(prefs)
                }
            };
        }

        [Fact]
        public async Task OtherPreference_Invokes_StampPNRs()
        {
            _stampingHandler.Reset();
            var provider = _serviceProvider.GetRequiredService<IEnrolmentProvider>();

            var prefs = new List<AccountPreference>
            {
                new AccountPreference { PreferenceType = AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions, PreferenceValue = "Y" }
            };

            var request = BuildRequest(prefs);

            await provider.EnrolFcMember(request);

            Assert.Equal(1, _stampingHandler.InvocationCount);
        }

        [Fact]
        public async Task PreferredPartnerFFP_WithValue_DoesNotInvoke_StampPNRs()
        {
            _stampingHandler.Reset();
            var provider = _serviceProvider.GetRequiredService<IEnrolmentProvider>();

            var prefs = new List<AccountPreference>
            {
                new AccountPreference { PreferenceType = AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP, PreferenceValue = "DL" }
            };

            var request = BuildRequest(prefs);

            await provider.EnrolFcMember(request);

            Assert.Equal(0, _stampingHandler.InvocationCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task PreferredPartnerFFP_NullOrEmpty_Invokes_StampPNRs(string? preferenceValue)
        {
            _stampingHandler.Reset();
            var provider = _serviceProvider.GetRequiredService<IEnrolmentProvider>();
            var prefs = new List<AccountPreference>
             {
                 new AccountPreference { PreferenceType = AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP, PreferenceValue = preferenceValue! }
             };

            var request = BuildRequest(prefs);

            await provider.EnrolFcMember(request);

            Assert.Equal(1, _stampingHandler.InvocationCount);
        }

        [Fact]
        public async Task EnrolYoungFlyerMember_ForcesActiveStatusForIdentityAndResponse()
        {
            var request = BuildRequest(null);
            request.CustomerDetails!.Status = "Active Pending";
            var response = await _serviceProvider.GetRequiredService<IEnrolmentProvider>().EnrolYoungFlyerMember(request);

            Assert.Equal("Active", request.CustomerDetails.Status);
            Assert.Equal("Active", response.MemberDetails!.Status);
        }

        [Fact]
        public async Task EnrolFcMember_SerializesOnlyProtectedPreferencesAsDisabled_ForLegacyIosCaller()
        {
            ConfigReader.Set(Environment.DeDupFCGenUrl, "https://mobile-enrolment.test/GenerateFcNumber");
            var responsesBefore = _httpClientFactory.GetResponses(ConfigReader.Get(Environment.DeDupFCGenUrl)).Count;
            SetChannel("mobile;platform=ios;build-id=1.9.1", "STITCH", "GRAPHQL");
            var request = BuildRequest(CreatePreferences());

            await _serviceProvider.GetRequiredService<IEnrolmentProvider>().EnrolFcMember(request);

            await AssertSerializedMarketingPreferences(responsesBefore, "N");
        }

        [Fact]
        public async Task EnrolFcMember_PreservesProtectedPreferences_ForSupportedIosCaller()
        {
            ConfigReader.Set(Environment.DeDupFCGenUrl, "https://mobile-enrolment.test/GenerateFcNumber");
            var responsesBefore = _httpClientFactory.GetResponses(ConfigReader.Get(Environment.DeDupFCGenUrl)).Count;
            SetChannel("mobile;platform=ios;build-id=999.0.0", "STITCH", "GRAPHQL");
            var request = BuildRequest(CreatePreferences());

            await _serviceProvider.GetRequiredService<IEnrolmentProvider>().EnrolFcMember(request);

            await AssertSerializedMarketingPreferences(responsesBefore, "Y");
        }

        [Fact]
        public async Task EntraEnrolFcMember_SerializesOnlyProtectedPreferencesAsDisabled_ForLegacyAndroidCaller()
        {
            ConfigReader.Set(Environment.DeDupFCGenUrl, "https://mobile-enrolment.test/GenerateFcNumber");
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-enrol-success");
            var responsesBefore = _httpClientFactory.GetResponses(ConfigReader.Get(Environment.DeDupFCGenUrl)).Count;
            SetChannel("mobile;platform=android;build-id=1.9.9", "STITCH", "GRAPHQL");
            var request = BuildRequest(CreatePreferences());

            await _serviceProvider.GetRequiredService<IEnrolmentProvider>().EntraEnrolFcMember(request);

            await AssertSerializedMarketingPreferences(responsesBefore, "N");
        }

        public void Dispose() => AsyncContext.Current = null;

        private static IEnumerable<AccountPreference> CreatePreferences() =>
        [
            new() { PreferenceType = AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, PreferenceValue = "Y" },
            new() { PreferenceType = AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions, PreferenceValue = "Y" },
            new() { PreferenceType = AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions, PreferenceValue = "Y" },
            new() { PreferenceType = AccountPreference.AccountPreferenceTypes.EmailCommunication, PreferenceValue = "Y" }
        ];

        private async Task AssertSerializedMarketingPreferences(int responsesBefore, string expectedProtectedPreferenceValue)
        {
            var endpoint = ConfigReader.Get(Environment.DeDupFCGenUrl);
            var response = _httpClientFactory.GetResponses(endpoint)
                .Skip(responsesBefore)
                .Single();
            var payload = JObject.Parse(await response.RequestMessage!.Content!.ReadAsStringAsync());
            var preferences = payload.SelectToken("deDupRequest.data.customerDetails.preferences")!
                .ToObject<List<AccountPreference>>()!;
            var protectedPreferences = preferences.Where(preference => preference.PreferenceType is
                AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions or
                AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions or
                AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions)
                .ToList();

            Assert.Equal(3, protectedPreferences.Count);
            Assert.All(protectedPreferences, preference => Assert.Equal(expectedProtectedPreferenceValue, preference.PreferenceValue));
            Assert.Equal("Y", preferences.Single(preference =>
                preference.PreferenceType == AccountPreference.AccountPreferenceTypes.EmailCommunication).PreferenceValue);
        }

        private static void SetChannel(params string[] values)
        {
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(values);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }

        private sealed class TestStampingPNRsHandler : IStampingPNRsHandler
        {
            public int InvocationCount { get; private set; }

            public Task StampPNRs(CustomerDTO customerResponse)
            {
                InvocationCount++;
                return Task.CompletedTask;
            }

            public void Reset() => InvocationCount = 0;
        }
    }
}
