using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class MobileEnrolmentMarketingPreferencePolicyTests : IDisposable
    {
        public MobileEnrolmentMarketingPreferencePolicyTests()
        {
            ConfigReader.Set(Environment.MobileIosMinimumEnrolmentVersion, "1.10.0");
            ConfigReader.Set(Environment.MobileAndroidMinimumEnrolmentVersion, "1.10.0");
            AsyncContext.Current = null;
        }

        [Fact]
        public void Apply_DisablesAndAddsOnlyProtectedPreferences_ForLegacyIosVersion()
        {
            SetChannel("MOBILE; platform=IOS;build-id=1.9.1(2)", "STITCH", "GRAPHQL");
            var request = CreateRequest(
                ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"),
                new AccountPreference
                {
                    PreferenceType = AccountPreference.AccountPreferenceTypes.EmailCommunication,
                    PreferenceValue = "Y"
                });

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
            Assert.Equal("Y", GetPreference(request, AccountPreference.AccountPreferenceTypes.EmailCommunication));
            Assert.Equal(new[] { "MOBILE; platform=IOS;build-id=1.9.1(2)", "STITCH", "GRAPHQL" }, AsyncContext.Current!.Channel);
        }

        [Theory]
        [InlineData("ios", "1.10.0")]
        [InlineData("ios", "1.10.1")]
        [InlineData("android", "1.10.0")]
        [InlineData("android", "1.10.1")]
        public void Apply_PreservesPreferences_ForSupportedVersions(string platform, string buildId)
        {
            SetChannel($"mobile;platform={platform};build-id={buildId}");
            var request = CreateRequest(
                ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"),
                ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions, "Y"),
                ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            Assert.All(request.CustomerDetails!.Preferences!, preference => Assert.Equal("Y", preference.PreferenceValue));
        }

        [Fact]
        public void Apply_UsesNumericVersionOrdering()
        {
            ConfigReader.Set(Environment.MobileIosMinimumEnrolmentVersion, "1.9.1");
            SetChannel("mobile;platform=ios;build-id=1.10.0");
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            Assert.Equal("Y", GetPreference(request, AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions));
        }

        [Fact]
        public void Apply_UsesParenthesizedBuildAsTheFinalVersionComponent()
        {
            ConfigReader.Set(Environment.MobileIosMinimumEnrolmentVersion, "1.10.0(2)");
            SetChannel("mobile;platform=ios;build-id=1.10.0(1)");
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Fact]
        public void Apply_PreservesPreferences_ForNonMobileChannel()
        {
            SetChannel("STITCH", "GRAPHQL");
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            Assert.Equal("Y", GetPreference(request, AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions));
        }

        [Fact]
        public void Apply_DisablesProtectedPreferences_WhenAsyncContextIsUnavailable()
        {
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Fact]
        public void Apply_DisablesAndAddsProtectedPreferences_WhenTheChannelCollectionIsEmpty()
        {
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel([]);
            var request = CreateRequest();

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Fact]
        public void Apply_DisablesAndAddsProtectedPreferences_WhenThePreferenceCollectionIsNull()
        {
            SetChannel("mobile;platform=ios;build-id=1.9.1");
            var request = new CreateCustomerAccountRequest { CustomerDetails = new CustomerAccountDetails() };

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Fact]
        public void Apply_DisablesProtectedPreferences_ForMalformedMinimumVersionConfiguration()
        {
            ConfigReader.Set(Environment.MobileIosMinimumEnrolmentVersion, "not-a-version");
            SetChannel("mobile;platform=ios;build-id=1.10.0");
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Theory]
        [InlineData(" 1.10.0")]
        [InlineData("1.10.0 ")]
        public void Apply_DisablesProtectedPreferences_ForMinimumVersionConfigurationWithSurroundingWhitespace(string minimumVersion)
        {
            ConfigReader.Set(Environment.MobileIosMinimumEnrolmentVersion, minimumVersion);
            SetChannel("mobile;platform=ios;build-id=1.10.0");
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Theory]
        [InlineData("mobile;platform=android;build-id=1.9.9")]
        [InlineData("mobile;platform=android;build-id=not-a-version")]
        [InlineData("mobile;platform=android;build-id=2.5.0 ")]
        [InlineData("mobile;platform=android;build-id=2.5.0(-1)")]
        [InlineData("mobile;platform=android;build-id=2.5.+1")]
        [InlineData("mobile;platform=android;build-id=2. 5.0")]
        [InlineData("mobile;platform=android;build-id=2.5.0(+1)")]
        [InlineData("mobile;platform=windows;build-id=2.5.0")]
        [InlineData("mobile;build-id=2.5.0")]
        [InlineData("mobile;platform=android")]
        [InlineData("mobile;platform=android;platform=ios;build-id=2.5.0")]
        [InlineData("mobile;platform=android;build-id=2.5.0;build-id=2.5.1")]
        public void Apply_DisablesProtectedPreferences_ForUnevaluableMobileContext(string channel)
        {
            SetChannel(channel);
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        [Fact]
        public async Task Apply_DisablesProtectedPreferences_ForBuildIdWithLeadingWhitespaceInRequestHeader()
        {
            const string channel = "mobile;platform=ios;build-id= 1.10.0";
            var functionContext = new TestFunctionContext();
            var requestData = new TestHttpRequestData(
                functionContext,
                new Uri("https://mobile-enrolment.test"),
                headers: new Dictionary<string, string> { [AsyncContext.APIChannelHeader] = channel });
            functionContext.Features.Set<Microsoft.Azure.Functions.Worker.Http.HttpRequestData>(requestData);
            AsyncContext.Current = await AsyncContext.CreateAsync(functionContext);
            var request = CreateRequest(ProtectedPreference(AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions, "Y"));

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);

            AssertProtectedPreferencesAreDisabled(request);
        }

        public void Dispose()
        {
            AsyncContext.Current = null;
        }

        private static CreateCustomerAccountRequest CreateRequest(params AccountPreference[] preferences) => new()
        {
            CustomerDetails = new CustomerAccountDetails { Preferences = preferences.ToList() }
        };

        private static AccountPreference ProtectedPreference(AccountPreference.AccountPreferenceTypes preferenceType, string value) => new()
        {
            PreferenceType = preferenceType,
            PreferenceValue = value
        };

        private static string? GetPreference(CreateCustomerAccountRequest request, AccountPreference.AccountPreferenceTypes preferenceType) =>
            request.CustomerDetails!.Preferences!.Single(preference => preference.PreferenceType == preferenceType).PreferenceValue;

        private static void AssertProtectedPreferencesAreDisabled(CreateCustomerAccountRequest request)
        {
            Assert.Equal("N", GetPreference(request, AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions));
            Assert.Equal("N", GetPreference(request, AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions));
            Assert.Equal("N", GetPreference(request, AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions));
        }

        private static void SetChannel(params string[] values)
        {
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(values);
        }
    }
}
