using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
public sealed class InflightEnrolCustomerTests : IClassFixture<MemoryStoreFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
{
    private const string EnrollmentChannel = "WEB";
    private const string MediaCode = "SLK";
    private const string CustomerType = "Basic";
    private const string PreferenceType = "VaaMarketingPermissions";
    private const string PreferenceValue = "Y";
    private static readonly string env = Config.Environment;
    private readonly MemoryStoreFixture memoryStoreFixture;
    private readonly TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService;

    public InflightEnrolCustomerTests(MemoryStoreFixture memoryStoreFixture, TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService)
    {
        this.memoryStoreFixture = memoryStoreFixture;
        memoryStoreFixture.SetOnce("MYA-Password", () => Vault.Default.GetSecretValue("MYA-Password"));
        this.otpRequestTableStorageService = otpRequestTableStorageService;
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC01_EnrolsNewInflightCustomer()
    {
        string firstName = $"Inflight{DataGenerator.RandomString(5)}";
        string lastName = $"Member{DataGenerator.RandomString(5)}";
        string emailId = $"inflight{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        // A unique email and its fresh OTP make each enrolment independent during parallel runs.
        string enrolmentToken = await EnrolmentHelper.ReturnEnrolmentToken(otpRequestTableStorageService, emailId);

        using HttpResponseMessage enrolmentResponse = GraphQL.InflightEnrolCustomer(env, EnrollmentChannel, enrolmentToken, firstName, lastName, CustomerType, EnrollmentChannel, MediaCode, emailId, PreferenceType, PreferenceValue).EnsureGQLResponseSuccess();
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());

        string enrolmentStatus = enrolmentResult.SelectToken("data.inflightEnrolCustomer")?.ToString() ?? "<missing>";
        Assert.True(enrolmentStatus.Is("Ok"), $"Email '{emailId}': expected in-flight enrolment status 'Ok', but found '{enrolmentStatus}'.");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC02_EnrolsNewInflightCustomerAndRegistersDevice()
    {
        string firstName = $"Inflight{DataGenerator.RandomString(5)}";
        string lastName = $"Member{DataGenerator.RandomString(5)}";
        string emailId = $"inflight{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        // A unique email and its fresh OTP make this enrolment independent from other in-flight runs.
        string enrolmentToken = await EnrolmentHelper.ReturnEnrolmentToken(otpRequestTableStorageService, emailId);

        using HttpResponseMessage enrolmentResponse = GraphQL.InflightEnrolCustomer(env, EnrollmentChannel, enrolmentToken, firstName, lastName, CustomerType, EnrollmentChannel, MediaCode, emailId, PreferenceType, PreferenceValue).EnsureGQLResponseSuccess();
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
        string enrolmentStatus = enrolmentResult.SelectToken("data.inflightEnrolCustomer")?.ToString() ?? "<missing>";
        Assert.True(enrolmentStatus.Is("Ok"), $"Email '{emailId}': expected in-flight enrolment status 'Ok' before device registration, but found '{enrolmentStatus}'.");

        using HttpResponseMessage deviceResponse = GraphQL.SetDeviceInflightState(env).EnsureGQLResponseSuccess();
        JObject deviceResult = JObject.Parse(await deviceResponse.Content.ReadAsStringAsync());

        string deviceStatus = deviceResult.SelectToken("data.setDeviceInflightState.status")?.ToString() ?? "<missing>";
        Assert.True(deviceStatus.Is("Registered"), $"Email '{emailId}': expected the in-flight device status 'Registered', but found '{deviceStatus}'.");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC03_RejectsAlreadyRegisteredEmail()
    {
        Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints("Gold");
        string emailId = loyaltyDetails["email"];
        string enrolmentToken = await EnrolmentHelper.ReturnEnrolmentToken(otpRequestTableStorageService, emailId);

        using HttpResponseMessage enrolmentResponse = GraphQL.InflightEnrolCustomer(env, EnrollmentChannel, enrolmentToken, "Inflight", "Member", CustomerType, EnrollmentChannel, MediaCode, emailId, PreferenceType, PreferenceValue);
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(enrolmentResponse.IsSuccessStatusCode, $"Email '{emailId}': expected GraphQL HTTP 200 with a duplicate-email error envelope, found HTTP {(int)enrolmentResponse.StatusCode}.");
        JToken? enrolmentData = enrolmentResult.SelectToken("data.inflightEnrolCustomer");
        string duplicateEmailMessage = enrolmentResult.SelectToken("errors[0].message")?.ToString() ?? "<missing>";
        softAssert.True(enrolmentData is null or { Type: JTokenType.Null }, $"Email '{emailId}': expected no in-flight enrolment result for an existing account, but found token type '{enrolmentData?.Type.ToString() ?? "<missing>"}'.");
        softAssert.True(duplicateEmailMessage.Is("An account with that email address already exists."), $"Email '{emailId}': expected duplicate-email validation message 'An account with that email address already exists.', but found '{duplicateEmailMessage}'.");
        softAssert.AssertAll();
    }

    [Theory]
    [InlineData("Red")]
    [InlineData("Silver")]
    [InlineData("Gold")]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC04_RegistersDeviceForExistingMember(string tier)
    {
        Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints(tier);
        string memberNumber = loyaltyDetails["rowKey"];
        string emailId = loyaltyDetails["email"];
        string password = memoryStoreFixture.Get<string>("MYA-Password");

        using HttpResponseMessage signInResponse = GraphQL.AccountMemberSignIn(env, emailId, memberNumber, password).EnsureGQLResponseSuccess();
        JObject signInResult = JObject.Parse(await signInResponse.Content.ReadAsStringAsync());
        string accessToken = signInResult.RequireToken("data.accountMemberSignIn.accessToken").ToString();

        using HttpResponseMessage deviceResponse = GraphQL.AccountEnableWifiForDevice(env, accessToken).EnsureGQLResponseSuccess();
        JObject deviceResult = JObject.Parse(await deviceResponse.Content.ReadAsStringAsync());

        string deviceStatus = deviceResult.SelectToken("data.accountEnableIFWifiForDevice.status")?.ToString() ?? "<missing>";
        Assert.True(deviceStatus.Is("Registered"), $"Member '{memberNumber}': expected device registration status 'Registered' for {tier} tier, but found '{deviceStatus}'.");
    }
}
