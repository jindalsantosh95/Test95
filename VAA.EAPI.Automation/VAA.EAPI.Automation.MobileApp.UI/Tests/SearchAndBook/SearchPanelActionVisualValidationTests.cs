using VAA.EAPI.Automation.MobileApp.UI.Data;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SearchFlightsDisabledVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchFlightsDisabledVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Actions")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC11_SearchFlightsButton_MatchesDisabledFigmaState()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureSearchFlightsButton(accessibilityId),
            "search-book-search-flights-disabled-ios",
            nameof(SearchBookVisual_TC11_SearchFlightsButton_MatchesDisabledFigmaState));
}

public sealed class SearchFlightsEnabledVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchFlightsEnabledVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Actions")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC12_SearchFlightsButton_MatchesEnabledFigmaState()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureSearchFlightsButton(accessibilityId),
            "search-book-search-flights-enabled-ios",
            nameof(SearchBookVisual_TC12_SearchFlightsButton_MatchesEnabledFigmaState),
            prepare: PrepareValidSearch);

    private static void PrepareValidSearch(NativeFlightSearchPage page)
    {
        DateOnly departure = DateOnly.FromDateTime(DateTime.Today.AddDays(35));
        page.SelectAirports("LHR", "JFK");
        page.OpenDateSelector();
        page.SelectDate(departure);
        page.SelectDate(departure.AddDays(7));
        page.ConfirmDateSelection();
        Assert.True(page.IsSearchFlightsEnabled(), "Search Flights should be enabled for complete valid criteria.");
    }
}

public sealed class RewardSearchOffVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public RewardSearchOffVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Reward-Search")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC13_RewardSearchControl_MatchesOffFigmaState()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureRewardSearchControl(accessibilityId, enabled: false),
            "search-book-reward-search-off-ios",
            nameof(SearchBookVisual_TC13_RewardSearchControl_MatchesOffFigmaState));
}

public sealed class RewardSearchOnVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public RewardSearchOnVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Reward-Search")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC14_RewardSearchControl_MatchesOnFigmaState()
    {
        LoyaltyAccountProvider accounts = new(Fixture.Config);
        LoyaltyAccountCriteria criteria = new("SIT", "Red", MinimumPoints: 0);
        using LoyaltyAccountLease lease = accounts.Lease(criteria);
        LoyaltyLoginPage loginPage = new(Fixture.Driver);
        bool loggedIn = false;

        try
        {
            AssertSearchPanelComponent(
                (page, accessibilityId) => page.CaptureRewardSearchControl(accessibilityId, enabled: true),
                "search-book-reward-search-on-ios",
                nameof(SearchBookVisual_TC14_RewardSearchControl_MatchesOnFigmaState),
                prepareAuthenticatedState: () =>
                {
                    LoyaltyLoginResult login = loginPage.Login(
                        lease.Account.LoginIdentifier,
                        accounts.GetPassword());
                    loggedIn = true;
                    Assert.Contains(
                        lease.Account.FirstName,
                        login.Greeting,
                        StringComparison.OrdinalIgnoreCase);
                });
        }
        finally
        {
            if (loggedIn)
            {
                try
                {
                    loginPage.LogoutMember();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"Post-test member logout was unavailable: {exception.Message}");
                }
            }
        }
    }
}
