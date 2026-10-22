using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Data;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Account;

public sealed class MemberNotTravellingTests(ITestOutputHelper output) : SearchBookTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-Account-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Member-Not-Travelling")]
    [Trait("Platform", "iOS")]
    public void LoyaltySearchBook_TC04_MemberNotTravelling_ClearsProfileAndRetainsReplacementTraveller()
    {
        const string testName = nameof(LoyaltySearchBook_TC04_MemberNotTravelling_ClearsProfileAndRetainsReplacementTraveller);
        TrySetBrowserStackSessionName(testName);
        LoyaltyAccountProvider accounts = new(Fixture.Config);
        LoyaltyLoginPage login = new(Fixture.Driver);
        bool loggedIn = false;
        LoyaltyAccountLease? lease = null;
        try
        {
            lease = accounts.Lease(new LoyaltyAccountCriteria("SIT", "Red", MinimumPoints: 0));
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            login.Login(lease.Account.LoginIdentifier, accounts.GetPassword());
            loggedIn = true;
            NativeFlightSearchPage search = new(Fixture.Driver);
            search.Open();
            search.EnsureRewardSearchDisabled();
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, PassengerCounts.OneAdult));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(true, expectFlightSummary: true);
            NativeBookingPage booking = new(Fixture.Driver);
            booking.ContinueFromSummary();
            booking.WaitForMemberTravellerProfile(lease.Account.FirstName);
            MemberTravellerAssertions.MatchesMember(booking.ReadLeadTravellerEditor(), lease.Account);

            booking.SelectMemberNotTravelling();
            MemberTravellerAssertions.Cleared(booking.ReadLeadTravellerEditor());
            booking.EnterTravellerName(0, "Replacement", "Automation");
            booking.SelectDefaultTravellerDateOfBirth(0);
            booking.SelectTravellerGender(0, "Male");
            booking.EnterContactDetails("replacement.automation@example.com", "+44", "7700900123");
            IReadOnlyDictionary<string, string> replacement = booking.ReadLeadTravellerEditor();
            booking.SaveTravellerAndContinue(isFinalTraveller: true);

            MemberTravellerAssertions.Retained(replacement, booking.ReadLeadTravellerEditor());
            Assert.True(booking.IsMemberNotTravelling(), "Reopening the passenger reset the member selection.");
            TrySetBrowserStackStatus("passed", "Member opt-out cleared profile data and retained the replacement traveller; no payment attempted.", testName);
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
                { login.LogoutMember(); }
                catch (Exception) { Output.WriteLine("Post-test member logout was unavailable."); }
            }
            lease?.Dispose();
        }
    }
}
