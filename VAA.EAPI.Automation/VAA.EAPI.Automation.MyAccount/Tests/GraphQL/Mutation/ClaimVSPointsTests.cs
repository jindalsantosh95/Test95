using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Collection("Sequential")]
public sealed class ClaimVSPointsTests
{
    private const string ProcessedStatus = "Allocated";
    private const string NameMismatchErrorCode = "10175";
    private const string NameMismatchErrorMessage = "Name does not match with the FC account";
    private const string MissingTicketErrorMessage = "Unhandled exception (Value cannot be null. (Parameter 'ticketNumber'))";
    private const string MissingFcNumberErrorMessage = "Unhandled exception (Value cannot be null. (Parameter 'fcNumber'))";
    private const string ValidationErrorCode = "500";
    private static readonly string[] TierCodes = { "Gold", "Silver", "Red" };
    private static readonly string env = Config.Environment;

    [Fact]
    [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.SharedServicesSignOff)]
    [Trait("SharedService", "InRuleMS")]
    [Trait("SharedServiceConsumer", "MyAccount")]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC01_ReturnsClaimDetailsAndUpdatesPdmRecord()
    {
        (Dictionary<string, string> LoyaltyAccount, string Token) = await GetAuthenticatedLoyaltyAccountAsync();
        Dictionary<string, string> loyaltyAccount = LoyaltyAccount;
        string flyingClubNumber = loyaltyAccount["rowKey"];
        string firstName = loyaltyAccount["firstName"];
        string lastName = loyaltyAccount["lastName"];
        AzurePdmDbHelper azurePdmDbHelper = new();
        ClaimVsPointsFlownData flownData = AzurePdmDbHelper.CreateClaimData(firstName, lastName);
        bool claimCompleted = false;

        await azurePdmDbHelper.CreateFlownDataAsync(flownData);
        try
        {
            // The synthetic PDM row belongs to this member and must be allocated after the claim completes.
            using HttpResponseMessage response = GraphQL.ClaimVSPoints(env, Token, flownData.TicketNumber, flyingClubNumber).EnsureGQLResponseSuccess();
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            JToken segmentDetailsToken = responseBody.RequireToken("data.claimVSPoints.segmentDetails");
            Assert.True(segmentDetailsToken is JArray, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return segment details as an array, but found token type '{segmentDetailsToken.Type}'.");
            JArray segmentDetails = segmentDetailsToken as JArray ?? new JArray();
            ClaimVsPointsFlownData? persistedFlownData = await azurePdmDbHelper.RetrieveFlownDataAsync(flownData.TicketNumber);
            Assert.True(persistedFlownData is not null, $"Flying Club member '{flyingClubNumber}': expected PDM to contain ticket '{flownData.TicketNumber}' after the claim, but found '{(persistedFlownData is null ? "<missing>" : "<populated>")}'.");
            ClaimVsPointsFlownData assertedFlownData = persistedFlownData ?? new ClaimVsPointsFlownData(flownData.TicketNumber, string.Empty, string.Empty, DateTime.MinValue);
            string totalVirginPoints = responseBody.RequireToken("data.claimVSPoints.totalVirginPoints").ToString();
            string totalTierPoints = responseBody.RequireToken("data.claimVSPoints.totalTierPoints").ToString();
            string totalBonusPoints = responseBody.RequireToken("data.claimVSPoints.totalBonusPoints").ToString();
            bool virginPointsParsed = int.TryParse(totalVirginPoints, out int totalVirginPointsValue);
            bool tierPointsParsed = int.TryParse(totalTierPoints, out int totalTierPointsValue);
            bool bonusPointsParsed = int.TryParse(totalBonusPoints, out int totalBonusPointsValue);
            int segmentVirginPoints = 0;
            int segmentTierPoints = 0;
            int segmentBonusPoints = 0;
            SoftAssertHelper softAssert = new();

            softAssert.True(segmentDetails.Count > 0, $"Flying Club member '{flyingClubNumber}': expected at least one claimed segment for ticket '{flownData.TicketNumber}', but found count '{segmentDetails.Count}'.");
            softAssert.True(virginPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' Virgin Points '{totalVirginPoints}' to be numeric.");
            softAssert.True(tierPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' Tier Points '{totalTierPoints}' to be numeric.");
            softAssert.True(bonusPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' Bonus Points '{totalBonusPoints}' to be numeric.");
            softAssert.True(assertedFlownData.MemberNumber.Is(flyingClubNumber), $"Flying Club member '{flyingClubNumber}': expected PDM ticket '{flownData.TicketNumber}' to store the claiming member number.");
            softAssert.True(assertedFlownData.ProcessingStatus.Is(ProcessedStatus), $"Flying Club member '{flyingClubNumber}': expected PDM ticket '{flownData.TicketNumber}' to have processing status '{ProcessedStatus}', but found '{assertedFlownData.ProcessingStatus ?? "<null>"}'.");

            foreach (JToken segmentDetail in segmentDetails)
            {
                string operatingAirlineCode = segmentDetail.RequireToken("operatingAirlineCode").ToString();
                string flightNumber = segmentDetail.RequireToken("flightNumber").ToString();
                string originAirportCode = segmentDetail.RequireToken("originAirportCode").ToString();
                string destinationAirportCode = segmentDetail.RequireToken("destinationAirportCode").ToString();
                string dateOfFlightValue = segmentDetail.RequireToken("dateOfFlight").ToString();
                string virginPoints = segmentDetail.RequireToken("virginPoints").ToString();
                string tierPoints = segmentDetail.RequireToken("tierPoints").ToString();
                string bonusPoints = segmentDetail.RequireToken("bonusPoints").ToString();
                bool dateOfFlightParsed = DateTime.TryParse(dateOfFlightValue, out DateTime dateOfFlight);
                bool segmentVirginPointsParsed = int.TryParse(virginPoints, out int segmentVirginPointsValue);
                bool segmentTierPointsParsed = int.TryParse(tierPoints, out int segmentTierPointsValue);
                bool segmentBonusPointsParsed = int.TryParse(bonusPoints, out int segmentBonusPointsValue);
                segmentVirginPoints += segmentVirginPointsValue;
                segmentTierPoints += segmentTierPointsValue;
                segmentBonusPoints += segmentBonusPointsValue;

                softAssert.True(operatingAirlineCode.Is(assertedFlownData.OperatingAirlineCode), $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return operating airline '{assertedFlownData.OperatingAirlineCode}', but found '{operatingAirlineCode}'.");
                softAssert.True(flightNumber.Is($"{assertedFlownData.OperatingAirlineCode}{assertedFlownData.OperatingFlightNumber}"), $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return flight '{assertedFlownData.OperatingAirlineCode}{assertedFlownData.OperatingFlightNumber}', but found '{flightNumber}'.");
                softAssert.True(originAirportCode.Is(assertedFlownData.OriginPort), $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return origin '{assertedFlownData.OriginPort}', but found '{originAirportCode}'.");
                softAssert.True(destinationAirportCode.Is(assertedFlownData.DestinationPort), $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return destination '{assertedFlownData.DestinationPort}', but found '{destinationAirportCode}'.");
                softAssert.True(dateOfFlightParsed && dateOfFlight.Date == assertedFlownData.DepartureDate.Date, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' to return departure date '{assertedFlownData.DepartureDate:yyyy-MM-dd}', but found '{dateOfFlightValue}'.");
                softAssert.True(segmentVirginPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' segment Virgin Points '{virginPoints}' to be numeric.");
                softAssert.True(segmentTierPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' segment Tier Points '{tierPoints}' to be numeric.");
                softAssert.True(segmentBonusPointsParsed, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' segment Bonus Points '{bonusPoints}' to be numeric.");
                bool isPartialTransaction = segmentDetail.RequireToken("isPartialTransaction").Value<bool>();
                softAssert.True(!isPartialTransaction, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' segment processing to complete without a partial transaction, but found isPartialTransaction='{isPartialTransaction}'.");
            }

            softAssert.True(totalVirginPointsValue == segmentVirginPoints, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' total Virgin Points '{totalVirginPoints}' to equal segment points '{segmentVirginPoints}'.");
            softAssert.True(totalTierPointsValue == segmentTierPoints, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' total Tier Points '{totalTierPoints}' to equal segment points '{segmentTierPoints}'.");
            softAssert.True(totalBonusPointsValue == segmentBonusPoints, $"Flying Club member '{flyingClubNumber}': expected ticket '{flownData.TicketNumber}' total Bonus Points '{totalBonusPoints}' to equal segment points '{segmentBonusPoints}'.");
            softAssert.AssertAll();
            claimCompleted = true;
        }
        finally
        {
            // Retain successful claim evidence, but remove the synthetic PDM row when the claim fails.
            if (!claimCompleted)
                await azurePdmDbHelper.DeleteFlownDataAsync(flownData.TicketNumber);
        }
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    public async Task TC02_RejectsClaimForMismatchedAccountName()
    {
        (Dictionary<string, string> ticketOwner, string _) = await GetAuthenticatedLoyaltyAccountAsync();
        (Dictionary<string, string> claimingAccount, string token) = await GetAuthenticatedLoyaltyAccountAsync(ticketOwner["firstName"], ticketOwner["lastName"]);
        AzurePdmDbHelper azurePdmDbHelper = new();
        ClaimVsPointsFlownData flownData = AzurePdmDbHelper.CreateClaimData(ticketOwner["firstName"], ticketOwner["lastName"]);

        await azurePdmDbHelper.CreateFlownDataAsync(flownData);
        try
        {
            // A different stored member proves the mutation rejects a valid ticket whose PDM names do not match the claimant.
            using HttpResponseMessage response = GraphQL.ClaimVSPoints(env, token, flownData.TicketNumber, claimingAccount["rowKey"]);
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            string errorMessage = responseBody.RequireToken("errors[0].message").ToString();
            string errorCode = responseBody.RequireToken("errors[0].extensions.code").ToString();
            string exceptionMessage = responseBody.RequireToken("errors[0].extensions.Exception").ToString();
            SoftAssertHelper softAssert = new();

            softAssert.True(response.IsSuccessStatusCode, $"Flying Club member '{claimingAccount["rowKey"]}': expected ticket '{flownData.TicketNumber}' name-mismatch response to use HTTP 200, but found HTTP {(int)response.StatusCode}.");
            softAssert.True(errorMessage.Is(NameMismatchErrorMessage), $"Flying Club member '{claimingAccount["rowKey"]}': expected ticket '{flownData.TicketNumber}' name-mismatch message '{NameMismatchErrorMessage}', but found '{errorMessage}'.");
            softAssert.True(errorCode.Is(NameMismatchErrorCode), $"Flying Club member '{claimingAccount["rowKey"]}': expected ticket '{flownData.TicketNumber}' name-mismatch code '{NameMismatchErrorCode}', but found '{errorCode}'.");
            softAssert.True(exceptionMessage.Is(NameMismatchErrorMessage), $"Flying Club member '{claimingAccount["rowKey"]}': expected ticket '{flownData.TicketNumber}' name-mismatch exception '{NameMismatchErrorMessage}', but found '{exceptionMessage}'.");
            softAssert.AssertAll();
        }
        finally
        {
            // A rejected name mismatch cannot update PDM, so always remove this synthetic row.
            await azurePdmDbHelper.DeleteFlownDataAsync(flownData.TicketNumber);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC03_RejectsMissingRequiredArgument(bool ticketNumberIsMissing)
    {
        (Dictionary<string, string> LoyaltyAccount, string Token) = await GetAuthenticatedLoyaltyAccountAsync();
        Dictionary<string, string> loyaltyAccount = LoyaltyAccount;
        string ticketNumber = ticketNumberIsMissing ? string.Empty : "9391408105129";
        string fcNumber = ticketNumberIsMissing ? loyaltyAccount["rowKey"] : string.Empty;
        string expectedMessage = ticketNumberIsMissing ? MissingTicketErrorMessage : MissingFcNumberErrorMessage;

        // Empty variables reach the mutation boundary and verify its required-input validation contract.
        using HttpResponseMessage response = GraphQL.ClaimVSPoints(env, Token, ticketNumber, fcNumber);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        string errorMessage = responseBody.RequireToken("errors[0].message").ToString();
        string errorCode = responseBody.RequireToken("errors[0].extensions.code").ToString();
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Flying Club member '{loyaltyAccount["rowKey"]}': expected missing {(ticketNumberIsMissing ? "ticket number" : "FC number")} to return an HTTP 200 GraphQL error envelope, but found HTTP {(int)response.StatusCode}.");
        softAssert.True(errorMessage.Is(expectedMessage), $"Flying Club member '{loyaltyAccount["rowKey"]}': expected missing {(ticketNumberIsMissing ? "ticket number" : "FC number")} message '{expectedMessage}', but found '{errorMessage}'.");
        softAssert.True(errorCode.Is(ValidationErrorCode), $"Flying Club member '{loyaltyAccount["rowKey"]}': expected missing {(ticketNumberIsMissing ? "ticket number" : "FC number")} code '{ValidationErrorCode}', but found '{errorCode}'.");
        softAssert.AssertAll();
    }

    private static async Task<(Dictionary<string, string> LoyaltyAccount, string Token)> GetAuthenticatedLoyaltyAccountAsync(string? excludedFirstName = null, string? excludedLastName = null)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            string tierCode = TierCodes[attempt % TierCodes.Length];
            Dictionary<string, string> loyaltyAccount = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints(tierCode);
            if (loyaltyAccount.ContainsKey("error"))
                continue;

            bool isExcludedAccount = loyaltyAccount["firstName"].Is(excludedFirstName) && loyaltyAccount["lastName"].Is(excludedLastName);
            if (isExcludedAccount)
                continue;

            // MFA-enabled storage accounts cannot provide an ID token for this non-OTP scenario, so select another candidate.
            string? token = await EntraTokenHelper.TryGetIdTokenAsync(env, loyaltyAccount["email"], loyaltyAccount["rowKey"], Application.LoginPassword);
            if (!token.IsNullOrWhiteSpace())
                return (loyaltyAccount, token);
        }

        Assert.Fail("Expected an eligible non-MFA loyalty account with the requested name constraints for Claim VS Points automation after ten attempts across Gold, Silver, and Red tiers.");
        return (new Dictionary<string, string>(), string.Empty);
    }
}
