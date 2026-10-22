using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class AccountMemberDetailsTests : IClassFixture<StoredLoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture;

        private static void AssertMemberDetailEquals(SoftAssertHelper softAssert, string fieldName, string expected, JToken? actualToken, bool redact = false)
        {
            // Mask customer identifiers and contact data while preserving an expected-versus-actual diagnostic for non-sensitive contract fields.
            if (actualToken is null || actualToken.Type == JTokenType.Undefined)
            {
                softAssert.True(false, $"Expected accountMemberDetails {fieldName} '{(redact ? (expected.IsNullOrWhiteSpace() ? "<empty>" : "<redacted>") : expected)}', but the response property was '<missing>'.");
                return;
            }

            // An explicit JSON null retains the legacy empty-value comparison, while an omitted property remains a contract failure.
            bool actualIsNull = actualToken.Type == JTokenType.Null;
            string actual = actualIsNull ? string.Empty : actualToken.ToString();
            string expectedDisplay = redact ? (expected.IsNullOrWhiteSpace() ? "<empty>" : "<redacted>") : expected;
            string actualDisplay = actualIsNull
                ? "<null>"
                : redact ? (actual.IsNullOrWhiteSpace() ? "<empty>" : "<redacted>") : (actual.IsNullOrWhiteSpace() ? "<empty>" : actual);

            softAssert.True(expected.Is(actual), $"Expected accountMemberDetails {fieldName} '{expectedDisplay}', but found '{actualDisplay}'.");
        }

        public AccountMemberDetailsTests(StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            this.loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
        }

        [Theory]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [InlineData("Red")]
        [InlineData("Silver")]
        [InlineData("Gold")]
        public async Task TC01_AccountMemberDetails_ReturnsAuthenticatedMember(string tierCode)
        {
            StoredLoyaltyMemberAccountDocuments loyaltyAccount = await loyaltyMemberAccountFixture.GetAccountDocumentsAsync(tierCode);

            HttpResponseMessage response = GraphQL.AccountMemberDetails(env, loyaltyAccount.Token).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(body), "Expected a non-empty accountMemberDetails response body.");

            JObject responseBody = JObject.Parse(body);
            JToken memberDetails = responseBody.RequireToken("data.accountMemberDetails.flyingClubMemberDetails");
            ContactDetails? primaryEmail = loyaltyAccount.CustomerDocument.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Email") && contact.Primary.Is("Y"));
            ContactDetails? primaryPhone = loyaltyAccount.CustomerDocument.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Phone") && contact.Primary.Is("Y"));
            Address? primaryAddress = loyaltyAccount.CustomerDocument.AddressDetails?.FirstOrDefault(address => address.Primary.Is("Y"));
            Preference? marketingPreference = loyaltyAccount.CustomerProfileDocument.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("VAAMarketingPermissions"));
            Preference? partnerPreference = loyaltyAccount.CustomerProfileDocument.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("PreferredPartnerFFP"));
            JToken? responsePrimaryAddress = memberDetails.SelectToken("address[0]");
            SoftAssertHelper softAssert = new();

            // The resolver turns an individual with a household head into a linked member.
            string expectedMemberType = loyaltyAccount.FlyingClubMemberDocument.MemberType.Is("household")
                ? "Household"
                : loyaltyAccount.FlyingClubMemberDocument.MemberType.Is("individual") && !loyaltyAccount.FlyingClubMemberDocument.HeadOfGroupMemberNumber.IsNullOrWhiteSpace()
                    ? "Linked"
                    : "Individual";

            AssertMemberDetailEquals(softAssert, "membership number", loyaltyAccount.MemberNumber, memberDetails.SelectToken("fCMembershipNumber"), redact: true);
            AssertMemberDetailEquals(softAssert, "customer ID", loyaltyAccount.CustomerDocument.CustomerId, memberDetails.SelectToken("customerID"), redact: true);
            AssertMemberDetailEquals(softAssert, "first name", loyaltyAccount.CustomerDocument.FirstName, memberDetails.SelectToken("firstName"), redact: true);
            AssertMemberDetailEquals(softAssert, "last name", loyaltyAccount.CustomerDocument.LastName, memberDetails.SelectToken("lastName"), redact: true);
            AssertMemberDetailEquals(softAssert, "gender", loyaltyAccount.CustomerDocument.Gender, memberDetails.SelectToken("gender"));
            AssertMemberDetailEquals(softAssert, "email", primaryEmail?.ContactValue ?? string.Empty, memberDetails.SelectToken("email"), redact: true);
            AssertMemberDetailEquals(softAssert, "phone number", primaryPhone?.ContactValue ?? string.Empty, memberDetails.SelectToken("contactNumber"), redact: true);
            AssertMemberDetailEquals(softAssert, "phone country code", primaryPhone?.CountryCode ?? string.Empty, memberDetails.SelectToken("countryCode"));

            AssertMemberDetailEquals(softAssert, "member type", expectedMemberType, memberDetails.SelectToken("memberType"));
            AssertMemberDetailEquals(softAssert, "tier code from FlyingClubMember", loyaltyAccount.FlyingClubMemberDocument.TierCode, memberDetails.SelectToken("tierCode"));
            AssertMemberDetailEquals(softAssert, "tier code from storage", tierCode, memberDetails.SelectToken("tierCode"));
            AssertMemberDetailEquals(softAssert, "head-of-group member number", loyaltyAccount.FlyingClubMemberDocument.HeadOfGroupMemberNumber ?? string.Empty, memberDetails.SelectToken("headOfGroupMemberNumber"), redact: true);

            if (loyaltyAccount.CustomerDocument.DateOfBirth.HasValue)
            {
                DateTime returnedDateOfBirth = (DateTime)memberDetails.RequireToken("dateofBirth");
                softAssert.AreEqual(loyaltyAccount.CustomerDocument.DateOfBirth.Value.Date, returnedDateOfBirth.Date, "Expected dateofBirth to match the Customer document.");
            }

            if (loyaltyAccount.FlyingClubMemberDocument.EnrollmentDate.HasValue)
            {
                DateTime returnedJoinDate = (DateTime)memberDetails.RequireToken("fCJoinDate");
                softAssert.AreEqual(loyaltyAccount.FlyingClubMemberDocument.EnrollmentDate.Value.Date, returnedJoinDate.Date, "Expected fCJoinDate to match the FlyingClubMember document.");
            }

            bool expectedMarketingPreference = marketingPreference?.PreferenceValue.Is("Y") == true;
            softAssert.AreEqual(expectedMarketingPreference, (bool)memberDetails.RequireToken("isVAAMarketingPreferred"), "Expected the marketing preference to match the CustomerProfile document.");
            AssertMemberDetailEquals(softAssert, "preferred partner FFP", partnerPreference?.PreferenceValue ?? string.Empty, memberDetails.SelectToken("preferredPartnerFFP"));

            // AccountMemberDetails returns only the Customer document's primary address.
            softAssert.True(primaryAddress is not null, "Expected a primary address in the Customer document.");
            softAssert.True(responsePrimaryAddress is not null, "Expected a primary address in accountMemberDetails.");
            if (primaryAddress is not null && responsePrimaryAddress is not null)
            {
                softAssert.True((primaryAddress.AddressType ?? string.Empty).Is(responsePrimaryAddress.RequireToken("addressType").ToString()), "Expected the primary address type to match the Customer document.");
                softAssert.True((primaryAddress.AddressLine1 ?? string.Empty).Is(responsePrimaryAddress.RequireToken("addressLine1").ToString()), "Expected primary address line 1 to match the Customer document.");
                softAssert.True((primaryAddress.City ?? string.Empty).Is(responsePrimaryAddress.RequireToken("city").ToString()), "Expected the primary address city to match the Customer document.");
                softAssert.True((primaryAddress.PostCode ?? string.Empty).Is(responsePrimaryAddress.RequireToken("postCode").ToString()), "Expected the primary address post code to match the Customer document.");
                softAssert.True((primaryAddress.Country ?? string.Empty).Is(responsePrimaryAddress.RequireToken("country").ToString()), "Expected the primary address country to match the Customer document.");
                softAssert.True((primaryAddress.Primary ?? string.Empty).Is(responsePrimaryAddress.RequireToken("primary").ToString()), "Expected the primary address flag to match the Customer document.");
            }

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [InlineData("Red")]
        [InlineData("Silver")]
        [InlineData("Gold")]
        public async Task TC02_AccountMemberDetails_ReturnsTierPointValuesForMemberTier(string tierCode)
        {
            StoredLoyaltyMemberAccountDocuments loyaltyAccount = await loyaltyMemberAccountFixture.GetAccountDocumentsAsync(tierCode);
            HttpResponseMessage response = GraphQL.AccountMemberDetails(env, loyaltyAccount.Token).EnsureGQLResponseSuccess();
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            JToken memberDetails = responseBody.RequireToken("data.accountMemberDetails.flyingClubMemberDetails");
            int retainTierPoints = memberDetails.RequireToken("retainTierPoints").Value<int>();
            int rollingTierPoints = memberDetails.RequireToken("rollingTierPoints").Value<int>();
            int tierPointsNeedToRetain = memberDetails.RequireToken("tierPointsNeedToRetain").Value<int>();
            int tierPointsNeedToReachNextTier = memberDetails.RequireToken("tierPointsNeedToReachNextTier").Value<int>();

            // Match the tier handler: upgrades use rolling points and retain values cannot be negative.
            if (tierCode.Is("Red"))
            {
                Assert.Equal(0, tierPointsNeedToRetain);
                Assert.Equal(400 - rollingTierPoints, tierPointsNeedToReachNextTier);
            }
            else if (tierCode.Is("Silver"))
            {
                Assert.Equal(Math.Max(400 - retainTierPoints, 0), tierPointsNeedToRetain);
                Assert.Equal(1000 - rollingTierPoints, tierPointsNeedToReachNextTier);
            }
            else
            {
                Assert.Equal(Math.Max(1000 - retainTierPoints, 0), tierPointsNeedToRetain);
                Assert.Equal(0, tierPointsNeedToReachNextTier);
            }
        }

        [Theory]
        [Trait("Category", Traits.LocalRun)]
        [InlineData("9106545529", "isLifeTimeGoldMember")]
        [InlineData("9106545503", "isMillionMiler")]
        [InlineData("1075810794", "is2MillionMiler")]
        [InlineData("1074728617", "isWingsMemeber")]
        public async Task TC03_AccountMemberDetails_ReturnsSpecialMemberStatus(string memberNumber, string statusField)
        {
            // Dedicated special-status members are local-only until the shared storage table exposes these flags.
            // Read the rotating password used by legacy accounts from Key Vault instead of source control.
            string legacyPassword = Vault.Default.GetSecretValue("MYA-LegacyPassword");
            string token = await EntraTokenHelper.GetIdTokenWithPasswordFallback(env, string.Empty, memberNumber, Application.LoginPassword, legacyPassword);
            HttpResponseMessage response = GraphQL.AccountMemberDetails(env, token).EnsureGQLResponseSuccess();
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            bool hasSpecialStatus = responseBody.RequireToken($"data.accountMemberDetails.flyingClubMemberDetails.memberStatus.{statusField}").Value<bool>();

            Assert.True(hasSpecialStatus, $"Expected member '{memberNumber}' to return {statusField}=true, but found '{hasSpecialStatus}'.");
        }

    }
}
