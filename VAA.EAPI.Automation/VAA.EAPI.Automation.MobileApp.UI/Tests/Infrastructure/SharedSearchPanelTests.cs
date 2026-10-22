using System.Reflection;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SharedSearchPanelTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Who, 1 person", 1, true)]
    [InlineData("Who, 5 people", 5, true)]
    [InlineData("15 people", 5, false)]
    [InlineData("6 people", 5, false)]
    [InlineData("5 travellers", 5, false)]
    public void IosPassengerSummary_MatchesAppCopyAndExactCount(string text, int count, bool expected)
        => Assert.Equal(expected, IosSearchPanelPage.MatchesPassengerTotal(text, count));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("-", "-", true, 26)]
    [InlineData("Saturday, 26 September 2026", "-", true, 1)]
    [InlineData("Saturday, 26 September 2026", "Thursday, 1 October 2026", true, 0)]
    [InlineData("Saturday, 26 September 2026", "Saturday, 26 September 2026", true, 26)]
    [InlineData("Friday, 25 September 2026", "-", true, 26)]
    [InlineData("Saturday, 26 September 2026", "-", false, 0)]
    [InlineData("-", "-", false, 26)]
    [InlineData("Saturday, September 26, 2026", "-", true, 1)]
    [InlineData("Saturday, September 26, 2026", "Thursday, October 1, 2026", true, 0)]
    public void CalendarDraft_SelectsOnlyRequiredEndpoint(string outbound, string inbound,
        bool roundTrip, int expectedDay)
    {
        DateOnly departure = new(2026, 9, 26);
        DateOnly? next = AndroidSearchPanelPage.NextDateSelection(departure,
            roundTrip ? new DateOnly(2026, 10, 1) : null, outbound, inbound);
        Assert.Equal(expectedDay, next?.Day ?? 0);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("Sat 26 September")]
    [InlineData("unexpected")]
    public void CalendarDraft_RejectsMissingOrIncompleteFullDate(string value)
        => Assert.Throws<InvalidOperationException>(() => AndroidSearchPanelPage.NextDateSelection(
            new DateOnly(2026, 9, 26), null, value, "-"));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void UnqualifiedJourneySwitch_IsSkippedAndOutsideReadinessPack()
    {
        MethodInfo method = typeof(SearchAndBook.SearchPanelJourneySwitchTests).GetMethod(
            nameof(SearchAndBook.SearchPanelJourneySwitchTests.SearchPanel_TC02_JourneyType_SwitchingToReturnRequiresReturnDate))
            ?? throw new InvalidOperationException("Journey-switch scenario was not found.");
        FactAttribute fact = Assert.IsType<FactAttribute>(method.GetCustomAttribute<FactAttribute>());
        Assert.Contains("PANEL-TC02", fact.Skip);
        Assert.Contains("date confirmation", fact.Skip);
        string[] categories = method.CustomAttributes.Where(attribute => attribute.AttributeType == typeof(TraitAttribute) &&
                Equals(attribute.ConstructorArguments[0].Value, "Category"))
            .Select(attribute => (string?)attribute.ConstructorArguments[1].Value ?? string.Empty).ToArray();
        Assert.Contains("MobileApp-SearchPanel-Pending", categories);
        Assert.DoesNotContain("MobileApp-SearchPanel-Readiness", categories);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void SelectableControlState_UsesCheckedValue(string value, bool expected)
        => Assert.Equal(expected, AndroidSearchPanelPage.ParseCheckedState(value));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    public void SelectableControlState_RejectsMissingOrInvalidState(string? value)
        => Assert.Throws<InvalidOperationException>(() => AndroidSearchPanelPage.ParseCheckedState(value));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Android", true)]
    [InlineData("android", true)]
    [InlineData("iOS", false)]
    public void Factory_SelectsOnlyTheConfiguredPlatform(string platform, bool android)
        => Assert.Equal(android, FlightSearchPanelPageFactory.IsAndroid(platform));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Factory_RejectsUnknownPlatform()
        => Assert.Throws<NotSupportedException>(() => FlightSearchPanelPageFactory.IsAndroid("Windows"));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SharedScenarios_HaveOneOwnerAndBothPlatformTraits()
    {
        Type[] classes = typeof(SharedSearchPanelTests).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(SearchPanelTestBase).IsAssignableFrom(type)).ToArray();
        Assert.Equal(5, classes.Length);
        foreach (Type type in classes)
        {
            Assert.Null(type.GetCustomAttribute<CollectionAttribute>());
            MethodInfo method = Assert.Single(type.GetMethods(), candidate => candidate.GetCustomAttribute<FactAttribute>() is not null);
            string[] platforms = method.CustomAttributes.Where(attribute => attribute.AttributeType == typeof(TraitAttribute) &&
                Equals(attribute.ConstructorArguments[0].Value, "Platform"))
                .Select(attribute => (string?)attribute.ConstructorArguments[1].Value ?? string.Empty).Order().ToArray();
            Assert.Equal(new[] { "Android", "iOS" }, platforms);
        }
        Assert.DoesNotContain(typeof(SharedSearchPanelTests).Assembly.GetTypes(),
            type => type.Name == "AndroidSearchRecentAirportTests");
        Assert.True(typeof(IFlightSearchPanelPage).IsAssignableFrom(typeof(AndroidSearchPanelPage)));
        Assert.True(typeof(IFlightSearchPanelPage).IsAssignableFrom(typeof(IosSearchPanelPage)));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void RecentFlow_VerifiesBothDirectionsAgainstOneBusinessContract()
        => RecentAirportSearchFlow.VerifyLatestFivePerDirection(new RecentPanel());

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void RecentFlow_RejectsIncorrectRetention()
        => Assert.ThrowsAny<Xunit.Sdk.XunitException>(() =>
            RecentAirportSearchFlow.VerifyLatestFivePerDirection(new RecentPanel { Retention = 6 }));

    private sealed class RecentPanel : IFlightSearchPanelPage
    {
        private readonly Dictionary<bool, List<string>> _history = new() { [true] = [], [false] = [] };
        public int Retention { get; init; } = 5;
        public void SelectAirport(bool origin, string code)
        {
            _history[origin].Remove(code);
            _history[origin].Insert(0, code);
            if (_history[origin].Count > Retention)
                _history[origin].RemoveAt(Retention);
        }
        public IReadOnlyList<string> ReadRecentAirports(bool origin) => _history[origin].ToArray();
        public void SelectRecentAirport(bool origin, string code) => SelectAirport(origin, code);
        public void Open() => throw new NotSupportedException();
        public void SetRoundTrip(bool roundTrip) => throw new NotSupportedException();
        public bool IsRoundTrip => throw new NotSupportedException();
        public bool SearchEnabled => throw new NotSupportedException();
        public void SetOneAdult() => throw new NotSupportedException();
        public bool HasAirport(bool origin, string code) => throw new NotSupportedException();
        public void SelectDates(DateOnly departure, DateOnly? returning = null) => throw new NotSupportedException();
        public bool HasDates(DateOnly departure, DateOnly? returning = null) => throw new NotSupportedException();
        public void OpenDates() => throw new NotSupportedException();
        public void ChooseDate(DateOnly date) => throw new NotSupportedException();
        public void CloseDates() => throw new NotSupportedException();
        public void OpenPassengers() => throw new NotSupportedException();
        public int PassengerCount(string type) => throw new NotSupportedException();
        public bool CanDecreasePassenger(string type) => throw new NotSupportedException();
        public void SetPassengerCount(string type, int desired) => throw new NotSupportedException();
        public void ConfirmPassengers() => throw new NotSupportedException();
        public void ClosePassengers() => throw new NotSupportedException();
        public bool HasPassengerTotal(int count) => throw new NotSupportedException();
    }
}
