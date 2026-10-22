using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Data;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Account;

public abstract class LoyaltyLoginTestBase : SearchBookTestBase
{
    protected LoyaltyLoginTestBase(ITestOutputHelper output) : base(output) { }

    protected void CompleteAuthenticatedBookingWithTier(
        string tier,
        int minimumPoints,
        [CallerMemberName] string testName = "")
        => CompleteAuthenticatedBookingWithTier(
            tier,
            minimumPoints,
            applyPayWithPoints: false,
            testName);

    protected void VerifyAuthenticatedCurrencyBookingOffersPoints(
        [CallerMemberName] string testName = "")
        => CompleteAuthenticatedBookingWithTier(
            tier: "Red",
            minimumPoints: 3_000,
            applyPayWithPoints: true,
            testName);

    protected void VerifyStoredMemberPersonalDetails(
        [CallerMemberName] string testName = "")
    {
        const string tier = "Red";
        TrySetBrowserStackSessionName(testName);
        LoyaltyLoginPage? loginPage = null;
        bool loggedIn = false;
        LoyaltyAccountProvider accounts = new(Fixture.Config);
        LoyaltyAccountCriteria criteria = new("SIT", tier, MinimumPoints: 0);
        using LoyaltyAccountLease lease = accounts.Lease(criteria);
        LoyaltyAccount account = lease.Account;

        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            Output.WriteLine(
                $"Validating Personal Details with reserved {account.Tier} account ending " +
                $"{LastFour(account.FlyingClubNumber)}.");

            loginPage = new LoyaltyLoginPage(Fixture.Driver);
            LoyaltyLoginResult login = loginPage.Login(account.LoginIdentifier, accounts.GetPassword());
            loggedIn = true;
            Assert.Contains(account.FirstName, login.Greeting, StringComparison.OrdinalIgnoreCase);

            LoyaltyPersonalDetailsPage personalDetailsPage = new(Fixture.Driver);
            personalDetailsPage.Open();
            LoyaltyPersonalDetails details = personalDetailsPage.ReadDetails();

            Assert.Contains(account.FirstName, details.MemberName, StringComparison.OrdinalIgnoreCase);
            Assert.True(details.EditDetailsAvailable, "Edit details was not available on Personal Details.");
            Assert.True(details.DeleteAccountAvailable, "Delete account was not available on Personal Details.");
            personalDetailsPage.ReturnToAccount();

            TrySetBrowserStackStatus(
                "passed",
                $"Stored {tier} member first name matched Personal Details and both account actions were available.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
        finally
        {
            if (loggedIn)
            {
                try
                {
                    loginPage!.LogoutMember();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"Post-test member logout was unavailable: {exception.Message}");
                }
            }
        }
    }

    private void CompleteAuthenticatedBookingWithTier(
        string tier,
        int minimumPoints,
        bool applyPayWithPoints,
        string testName)
    {
        TrySetBrowserStackSessionName(testName);
        LoyaltyLoginPage? loginPage = null;
        bool loggedIn = false;
        LoyaltyAccountProvider accounts = new(Fixture.Config);
        LoyaltyAccountCriteria criteria = new("SIT", tier, minimumPoints);
        using LoyaltyAccountLease lease = accounts.Lease(criteria);
        LoyaltyAccount account = lease.Account;
        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();

            Output.WriteLine(
                $"Logging in with reserved {account.Tier} test account ending {LastFour(account.FlyingClubNumber)} " +
                $"with {account.Points:N0} points.");
            loginPage = new LoyaltyLoginPage(Fixture.Driver);
            LoyaltyLoginResult result = loginPage.Login(account.LoginIdentifier, accounts.GetPassword());
            loggedIn = true;

            Assert.Equal(tier, account.Tier, ignoreCase: true);
            Assert.True(account.Points >= minimumPoints);
            Assert.False(result.Greeting.IsNullOrWhiteSpace());
            Assert.Contains(account.FirstName, result.Greeting, StringComparison.OrdinalIgnoreCase);

            PassengerCounts passengers = PassengerCounts.OneAdult;
            FlightSearch search = FlightSearch.OneWay(
                "LHR", "JFK", monthsFromToday: 3, passengers: passengers);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);
            searchPage.Open();
            if (applyPayWithPoints)
            {
                searchPage.EnsureRewardSearchDisabled();
            }
            else
            {
                searchPage.EnsureRewardSearchEnabled();
            }
            searchPage.SearchOneWay(search);
            searchPage.WaitForResults();

            SelectedFlight selected = searchPage.SelectFirstFlightByCarrier(
                virginFlight: true, expectFlightSummary: true, requiredCabin: applyPayWithPoints ? null : "Economy");
            FlightSummary summary = searchPage.ReadSummary();
            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(selected.CabinName, summary.FareText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Virgin Atlantic", selected.ResultText, StringComparison.OrdinalIgnoreCase);
            if (!applyPayWithPoints)
            {
                Assert.Equal("Economy", selected.CabinName, ignoreCase: true);
                int rewardCost = NativeFlightSearchPage.ReadRewardPoints(selected.FareLabel);
                int liveBalance = RedemptionAssertions.ReadBalance(result.PointsText);
                RedemptionAssertions.AssertAffordable(rewardCost, liveBalance);
                Assert.Equal(rewardCost, NativeFlightSearchPage.ReadRewardPoints(summary.PriceText));
                Assert.True(summary.ContinueEnabled, "An affordable redemption must allow checkout from Flight Summary.");
                Output.WriteLine($"Economy redemption: {rewardCost:N0} points; live member balance: {liveBalance:N0} points.");
            }

            NativeBookingCheckoutFlow checkout = new(Fixture.Driver);
            if (applyPayWithPoints)
            {
                SelectedPayWithPointsOption selection =
                    checkout.SelectPayWithPointsOptionForMember(passengers);
                Assert.False(selection.DisplayText.IsNullOrWhiteSpace());
                TrySetBrowserStackStatus(
                    "passed",
                    $"Stored {tier} loyalty account with at least {minimumPoints:N0} points logged in, " +
                    $"completed a Virgin Atlantic one-way search in {selected.CabinName}, opened Pay with " +
                    $"Points and selected '{selection.DisplayText}'. Applying points and HPP are out of scope.",
                    testName);
                return;
            }

            CompletedNativeBooking booking = checkout.CompleteForMember(passengers, account);
            Assert.False(booking.Confirmation.TotalPriceText.IsNullOrWhiteSpace());
            ReportSuccessfulBooking(
                booking,
                $"Stored {tier} loyalty account with at least " +
                $"{minimumPoints:N0} points logged in, completed a Virgin Atlantic one-way booking " +
                $"in {selected.CabinName}, paid, confirmed and found it in Trips.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
        finally
        {
            if (loggedIn)
            {
                try
                {
                    loginPage!.LogoutMember();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"Post-test member logout was unavailable: {exception.Message}");
                }
            }
        }
    }

    protected void VerifyLowBalanceMemberCannotContinueWithReturnUpperReward(
        [CallerMemberName] string testName = "")
    {
        const int maximumAccountPoints = 50_000;
        TrySetBrowserStackSessionName(testName);
        LoyaltyLoginPage? loginPage = null;
        bool loggedIn = false;
        LoyaltyAccountProvider accounts = new(Fixture.Config);
        LoyaltyAccountCriteria criteria = new(
            Environment: "SIT",
            Tier: "Red",
            MinimumPoints: 0,
            MaximumPoints: maximumAccountPoints);
        using LoyaltyAccountLease lease = accounts.Lease(criteria);
        LoyaltyAccount account = lease.Account;

        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            Output.WriteLine(
                $"Logging in with low-balance Red test account ending {LastFour(account.FlyingClubNumber)} " +
                $"with {account.Points:N0} points.");
            loginPage = new LoyaltyLoginPage(Fixture.Driver);
            LoyaltyLoginResult result = loginPage.Login(account.LoginIdentifier, accounts.GetPassword());
            loggedIn = true;

            Assert.InRange(account.Points, 0, maximumAccountPoints - 1);
            Assert.Contains(account.FirstName, result.Greeting, StringComparison.OrdinalIgnoreCase);

            FlightSearch search = FlightSearch.Return(
                "LHR", "JFK", monthsFromToday: 3, passengers: PassengerCounts.OneAdult);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);
            searchPage.Open();
            searchPage.EnsureRewardSearchEnabled();
            searchPage.SearchReturn(search);
            searchPage.WaitForResults();

            int outboundPoints = searchPage.SelectRewardFare(
                "Upper Class", expectFlightSummary: false);
            int inboundPointsAdjustment = searchPage.SelectRewardFare(
                "Upper Class", expectFlightSummary: true);
            int displayedJourneyPoints = checked(outboundPoints + inboundPointsAdjustment);
            InsufficientPointsSummary summary = searchPage.ReadInsufficientPointsSummary();
            Output.WriteLine(
                $"Displayed reward cost: outbound {outboundPoints:N0} + inbound adjustment " +
                $"{inboundPointsAdjustment:N0} " +
                $"= {displayedJourneyPoints:N0} points; account balance: {account.Points:N0} points.");

            Assert.True(
                displayedJourneyPoints > account.Points,
                $"Expected the displayed return Upper Class reward cost ({displayedJourneyPoints:N0}) " +
                $"to exceed the reserved account balance ({account.Points:N0}).");
            Assert.Contains("Insufficient points", summary.WarningText, StringComparison.OrdinalIgnoreCase);
            Assert.False(summary.ContinueEnabled);

            TrySetBrowserStackStatus(
                "passed",
                $"Low-balance Red member was blocked on Flight Summary: displayed return Upper " +
                $"Class reward cost {displayedJourneyPoints:N0} points exceeded the {account.Points:N0}-point balance.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
        finally
        {
            if (loggedIn)
            {
                try
                {
                    loginPage!.LogoutMember();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"Post-test member logout was unavailable: {exception.Message}");
                }
            }
        }
    }

    private static string LastFour(string value) =>
        value.Length <= 4 ? "****" : $"****{value[^4..]}";
}

public sealed class RepresentativeMemberLoyaltyBookingTests : LoyaltyLoginTestBase
{
    public RepresentativeMemberLoyaltyBookingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-Account-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "LoyaltySearchAndBook")]
    [Trait("Journey", "OneWay")]
    [Trait("AccountSelection", "RepresentativeMember")]
    [Trait("Readiness", "Ready")]
    [Trait("PaymentMode", "Redemption")]
    [Trait("Cabin", "Economy")]
    [Trait("Platform", "iOS")]
    public void LoyaltySearchBook_TC01_HighBalanceEconomyRedemption_CompletesThroughHppConfirmationAndTrips()
        => CompleteAuthenticatedBookingWithTier("Red", minimumPoints: 100_000);
}

public sealed class InsufficientPointsRewardBookingTests : LoyaltyLoginTestBase
{
    public InsufficientPointsRewardBookingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-Account-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "LoyaltySearchAndBook")]
    [Trait("Journey", "Return")]
    [Trait("Tier", "Red")]
    [Trait("Cabin", "UpperClass")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public void LoyaltySearchBook_TC02_LowBalanceReturnUpperClass_BlocksContinuationForInsufficientPoints()
        => VerifyLowBalanceMemberCannotContinueWithReturnUpperReward();
}

public sealed class PayWithPointsLoyaltyBookingTests : LoyaltyLoginTestBase
{
    public PayWithPointsLoyaltyBookingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-Account-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Deferred")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "LoyaltySearchAndBook")]
    [Trait("Journey", "OneWay")]
    [Trait("PaymentMode", "PayWithPoints")]
    [Trait("Tier", "Red")]
    [Trait("Readiness", "AppChangeRequired")]
    [Trait("Platform", "iOS")]
    public void LoyaltySearchBook_TC03_CurrencyBooking_SelectsDisplayedPayWithPointsOption()
        => VerifyAuthenticatedCurrencyBookingOffersPoints();
}

public sealed class LoyaltyPersonalDetailsTests : LoyaltyLoginTestBase
{
    public LoyaltyPersonalDetailsTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-Account-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "MyAccount")]
    [Trait("Area", "MyAccount")]
    [Trait("Feature", "MyAccount-PersonalDetails")]
    [Trait("Journey", "PersonalDetails")]
    [Trait("Tier", "Red")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public void LoyaltyAccount_TC01_StoredMemberFirstName_MatchesPersonalDetailsAndExposesAccountActions()
        => VerifyStoredMemberPersonalDetails();
}
