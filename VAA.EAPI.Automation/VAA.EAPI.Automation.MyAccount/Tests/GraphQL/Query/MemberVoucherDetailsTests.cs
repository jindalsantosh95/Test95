using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class MemberVoucherDetailsTests
    {
        private readonly string env = Config.Environment;

        [Theory]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", Traits.MemberVoucherDetails)]
        [InlineData("Red")]
        [InlineData("Silver")]
        [InlineData("Gold")]
        public async Task TC01_MemberVoucherDetails_ReturnsPagedVoucherResponse(string tierCode)
        {
            // Select a stored account with a voucher the GraphQL mapper will retain as currently valid.
            Dictionary<string, string> loyaltyVoucherAccount = RetrieveFromStorageTable.RetrieveLoyaltyVoucherAccountDetailsFromStorageTable(tierCode, minimumVoucherExpiryDate: DateTime.UtcNow.Date);
            Assert.False(loyaltyVoucherAccount.ContainsKey("error"), loyaltyVoucherAccount.GetValueOrDefault("error"));
            Assert.True(loyaltyVoucherAccount.TryGetValue("rowKey", out string? memberNumber) && !memberNumber.IsNullOrWhiteSpace(), $"Expected a voucher account for tier '{tierCode}'.");
            Assert.True(loyaltyVoucherAccount.TryGetValue("email", out string? email) && !email.IsNullOrWhiteSpace(), $"Expected an email address for voucher account tier '{tierCode}'.");

            string storedMemberNumber = memberNumber ?? string.Empty;
            string token = await EntraTokenHelper.GetIdToken(env, email ?? string.Empty, storedMemberNumber, Application.LoginPassword);
            HttpResponseMessage firstPageResponse = GraphQL.MemberVoucherDetails(env, token, page: 1, pageSize: 1).EnsureGQLResponseSuccess();
            string firstPageBody = await firstPageResponse.Content.ReadAsStringAsync();
            MemberVoucherDetailsResponse? firstPage = JsonConvert.DeserializeObject<MemberVoucherDetailsResponse>(firstPageBody);
            MemberVoucherDetails voucherDetails = Assert.IsType<MemberVoucherDetails>(firstPage?.Data?.MemberVoucherDetails);
            List<Category> categories = Assert.IsType<List<Category>>(voucherDetails.Categories);
            List<Voucher> vouchers = Assert.IsType<List<Voucher>>(voucherDetails.Vouchers);
            Voucher voucher = Assert.Single(vouchers);
            int totalVoucherCount = voucherDetails.MemberTotalVoucherCount.GetValueOrDefault();

            // Report non-sensitive voucher metadata while keeping the voucher number redacted from pipeline diagnostics.
            string categorySummary = string.Join(", ", categories.Select(category => $"{category.Code}:{category.SortOrder}"));
            Assert.True(totalVoucherCount > 0, $"Expected the voucher account to have at least one available voucher, but found total count '{totalVoucherCount}'.");
            Assert.True(totalVoucherCount >= vouchers.Count, $"Expected memberTotalVoucherCount '{totalVoucherCount}' to include current-page count '{vouchers.Count}'.");
            Assert.True(categories.Any(category => category.Code.Is("REWARD") && category.SortOrder == 0), $"Expected the REWARD voucher category with sort order 0, but found '{categorySummary}'.");
            Assert.True(categories.Any(category => category.Code.Is("CLUBHOUSE") && category.SortOrder == 1), $"Expected the CLUBHOUSE voucher category with sort order 1, but found '{categorySummary}'.");
            Assert.False(voucher.Type.IsNullOrWhiteSpace(), $"Expected voucher type, but found '{voucher.Type ?? "<empty>"}'.");
            Assert.False(voucher.Name.IsNullOrWhiteSpace(), $"Expected voucher name, but found '{voucher.Name ?? "<empty>"}'.");
            Assert.False(voucher.Source.IsNullOrWhiteSpace(), $"Expected voucher source, but found '{voucher.Source ?? "<empty>"}'.");
            Assert.False(voucher.Number.IsNullOrWhiteSpace(), $"Expected voucher number, but found '{(voucher.Number.IsNullOrWhiteSpace() ? "<empty>" : "<redacted>")}'.");
            Assert.False(voucher.Points.IsNullOrWhiteSpace(), $"Expected voucher points, but found '{voucher.Points ?? "<empty>"}'.");
            Assert.True(voucher.Category.IsAny("REWARD", "CLUBHOUSE"), $"Expected a supported voucher category, but found '{voucher.Category ?? "<empty>"}'.");
            Assert.True(voucher.Status.Is("Available"), $"Expected voucher status 'Available', but found '{voucher.Status ?? "<empty>"}'.");
            Assert.True(voucher.IssueDate <= DateTime.UtcNow, $"Expected voucher issue date not to be in the future, but found '{voucher.IssueDate:yyyy-MM-dd}'.");
            Assert.True(voucher.ExpiryDate.Date >= DateTime.UtcNow.Date, $"Expected voucher expiry date not to be in the past, but found '{voucher.ExpiryDate:yyyy-MM-dd}'.");
            Assert.True(voucherDetails.PointsThreshold.GetValueOrDefault() >= 0, $"Expected a non-negative points threshold, but found '{voucherDetails.PointsThreshold?.ToString() ?? "<missing>"}'.");

            if (totalVoucherCount > 1)
            {
                HttpResponseMessage secondPageResponse = GraphQL.MemberVoucherDetails(env, token, page: 2, pageSize: 1).EnsureGQLResponseSuccess();
                JObject secondPageBody = JObject.Parse(await secondPageResponse.Content.ReadAsStringAsync());
                string secondPageVoucherNumber = secondPageBody.RequireToken("data.memberVoucherDetails.vouchers[0].number").ToString();

                Assert.False(voucher.Number.Is(secondPageVoucherNumber), "Expected the second voucher page not to repeat the first voucher, but both voucher numbers resolved to '<redacted>'.");
            }

            HttpResponseMessage emptyPageResponse = GraphQL.MemberVoucherDetails(env, token, page: totalVoucherCount + 1, pageSize: 1).EnsureGQLResponseSuccess();
            JObject emptyPageBody = JObject.Parse(await emptyPageResponse.Content.ReadAsStringAsync());
            JArray emptyPageVouchers = Assert.IsType<JArray>(emptyPageBody.RequireToken("data.memberVoucherDetails.vouchers"));

            Assert.Empty(emptyPageVouchers);
            Assert.Equal(totalVoucherCount, emptyPageBody.RequireToken("data.memberVoucherDetails.memberTotalVoucherCount").Value<int>());
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", Traits.MemberVoucherDetails)]
        public async Task TC02_MemberVoucherDetails_ReturnsEmptyVouchersForStoredAccountWithoutVouchers()
        {
            Dictionary<string, string> loyaltyAccount = RetrieveFromStorageTable.RetrieveLoyaltyAccountWithoutVouchersFromStorageTable("Red", points: 0);
            Assert.False(loyaltyAccount.ContainsKey("error"), loyaltyAccount.GetValueOrDefault("error"));
            Assert.True(loyaltyAccount.TryGetValue("rowKey", out string? memberNumber) && !memberNumber.IsNullOrWhiteSpace(), "Expected a non-voucher loyalty account from storage.");
            Assert.True(loyaltyAccount.TryGetValue("email", out string? email) && !email.IsNullOrWhiteSpace(), "Expected an email address for the non-voucher loyalty account.");

            string token = await EntraTokenHelper.GetIdToken(env, email ?? string.Empty, memberNumber ?? string.Empty, Application.LoginPassword);
            HttpResponseMessage response = GraphQL.MemberVoucherDetails(env, token, page: 1, pageSize: 10).EnsureGQLResponseSuccess();
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray vouchers = Assert.IsType<JArray>(responseBody.RequireToken("data.memberVoucherDetails.vouchers"));

            Assert.Empty(vouchers);
            Assert.Equal(0, responseBody.RequireToken("data.memberVoucherDetails.memberTotalVoucherCount").Value<int>());
            Assert.Equal(0, responseBody.RequireToken("data.memberVoucherDetails.pointsThreshold").Value<int>());
        }

    }
}
