using System.Reflection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class MobileAutomationContractTests
{
    private static readonly string ContractPath = Path.Combine(
        AppContext.BaseDirectory,
        "Contracts",
        "search-book-automation-contract.json");
    private static readonly string CataloguePath = Path.Combine(
        AppContext.BaseDirectory,
        "TEST-CATALOG.md");
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void BrowserStackContentCapture_DefaultsToDisabled()
    {
        Assert.True(File.Exists(SettingsPath), $"Mobile automation settings not found: {SettingsPath}");
        JObject settings = JObject.Parse(File.ReadAllText(SettingsPath));
        JObject browserStack = Assert.IsType<JObject>(settings["BrowserStack"]);
        JValue captureNetworkContent = Assert.IsType<JValue>(browserStack["CaptureNetworkContent"]);

        Assert.False(captureNetworkContent.Value<bool>());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SelfHealing_RepeatedRecoveryWarningThreshold_IsConfigured()
    {
        Assert.True(File.Exists(SettingsPath), $"Mobile automation settings not found: {SettingsPath}");
        JObject settings = JObject.Parse(File.ReadAllText(SettingsPath));
        JObject selfHealing = Assert.IsType<JObject>(settings["SelfHealing"]);
        JValue warningThreshold = Assert.IsType<JValue>(selfHealing["WarningThreshold"]);

        Assert.True(
            warningThreshold.Value<int>() > 0,
            "SelfHealing:WarningThreshold must be greater than zero.");
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Select Upper Class, 545,000 points + £1,250", 545000)]
    [InlineData("Upper Class +0 points", 0)]
    [InlineData("Upper Class +25,000 points", 25000)]
    [InlineData("Upper Class -10,000 points", -10000)]
    public void RewardFareParser_ReturnsDisplayedPointsOrInboundAdjustment(
        string fareLabel,
        int expectedPoints)
        => Assert.Equal(expectedPoints, NativeFlightSearchPage.ReadRewardPoints(fareLabel));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("1 stop Flight 1 Direct Flight 2 Direct", 1)]
    [InlineData("2 connections Individual legs are Direct", 2)]
    [InlineData("Direct", 0)]
    public void SearchBookResultParser_PrefersItineraryStopCountOverIndividualDirectLegs(
        string cardText,
        int expectedConnections)
        => Assert.Equal(expectedConnections, NativeFlightSearchPage.ReadConnectionCount(cardText));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("1h 30m 1 stop", 1, true)]
    [InlineData("1h 30m 1 stop Flight path reports 0 stops", 1, true)]
    [InlineData("1h 30m Direct", 1, false)]
    [InlineData("1h 30m Direct", 0, true)]
    public void SearchBookResultCard_ContainsExpectedVisibleConnectionSummary(
        string cardText,
        int expectedConnections,
        bool expectedMatch)
        => Assert.Equal(
            expectedMatch,
            NativeFlightSearchPage.HasVisibleConnectionSummary(cardText, expectedConnections));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Direct flights", 0)]
    [InlineData("1 stop", 1)]
    [InlineData("2 connections", 2)]
    public void SearchBookConnectionFilter_UsesDisplayedLabelRatherThanInternalIdentifier(
        string filterLabel,
        int expectedConnections)
        => Assert.Equal(
            expectedConnections,
            NativeFlightSearchPage.ReadExpectedConnectionCount(filterLabel));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Air France, (8)", "Air France")]
    [InlineData("Virgin Atlantic (12)", "Virgin Atlantic")]
    [InlineData("KLM", "KLM")]
    public void SearchBookAirlineFilter_RemovesResultCountFromAccessibleLabel(
        string filterLabel,
        string expectedAirlineName)
        => Assert.Equal(expectedAirlineName, NativeFlightSearchPage.ReadAirlineName(filterLabel));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SearchBookContract_DefinesSupportedPlatformAndOwnedSelectors()
    {
        JObject contract = ReadContract();

        Assert.Equal(1, contract.Value<int>("schemaVersion"));
        Assert.Equal("supported", contract["platforms"]?.Value<string>("iOS"));
        Assert.Equal("planned", contract["platforms"]?.Value<string>("Android"));

        IReadOnlyDictionary<string, string> expectedSelectors = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["journeyTypePicker"] = NativeSearchAccessibilityIds.JourneyTypePicker,
            ["flyingFromButton"] = NativeSearchAccessibilityIds.FlyingFromButton,
            ["flyingToButton"] = NativeSearchAccessibilityIds.FlyingToButton,
            ["originAirportSearchField"] = NativeSearchAccessibilityIds.OriginAirportSearchField,
            ["destinationAirportSearchField"] = NativeSearchAccessibilityIds.DestinationAirportSearchField,
            ["whenButton"] = NativeSearchAccessibilityIds.WhenButton,
            ["whoButton"] = NativeSearchAccessibilityIds.WhoButton,
            ["searchFlightsButton"] = NativeSearchAccessibilityIds.SearchFlightsButton,
            ["rewardSearchToggle"] = NativeSearchAccessibilityIds.RewardSearchToggle,
            ["calendar"] = NativeSearchAccessibilityIds.Calendar,
            ["discountCodeButton"] = NativeSearchAccessibilityIds.DiscountCodeButton,
            ["discountCodeFieldContainer"] = NativeSearchAccessibilityIds.DiscountCodeFieldContainer,
            ["discountCodeTextField"] = NativeSearchAccessibilityIds.DiscountCodeTextField,
            ["discountCodeApplyButton"] = NativeSearchAccessibilityIds.DiscountCodeApplyButton,
            ["discountCodeClearButton"] = NativeSearchAccessibilityIds.DiscountCodeClearButton,
            ["discountCodeCancelButton"] = NativeSearchAccessibilityIds.DiscountCodeCancelButton,
            ["rewardSearchControl"] = NativeSearchAccessibilityIds.RewardSearchControl,
            ["passengerSelectorContainer"] = NativeSearchAccessibilityIds.PassengerSelectorContainer,
            ["passengerDetailsScrollView"] = NativeBookingAccessibilityIds.PassengerDetailsScrollView,
            ["passengerDetailsHub"] = NativeBookingAccessibilityIds.PassengerDetailsHub,
            ["redesignedPassengerDetailsSubmissionError"] = NativeBookingAccessibilityIds.RedesignedPassengerDetailsSubmissionError,
            ["redesignedPassengerEditorHeader"] = NativeBookingAccessibilityIds.RedesignedPassengerEditorHeader,
            ["redesignedPassengerEditorScrollView"] = NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView,
            ["redesignedPassengerSaveAndContinue"] = NativeBookingAccessibilityIds.RedesignedPassengerSaveAndContinue,
            ["redesignedPassengerSaveValidation"] = NativeBookingAccessibilityIds.RedesignedPassengerSaveValidation,
            ["redesignedPassengerDetailsContinueButton"] = NativeBookingAccessibilityIds.RedesignedPassengerDetailsContinueButton,
            ["redesignedBookingTermsToggle"] = NativeBookingAccessibilityIds.RedesignedBookingTermsToggle,
            ["redesignedDateOfBirthPicker"] = NativeBookingAccessibilityIds.RedesignedDateOfBirthPicker,
            ["redesignedContactEmailText"] = NativeBookingAccessibilityIds.RedesignedContactEmailText,
            ["redesignedContactCountryCode"] = NativeBookingAccessibilityIds.RedesignedContactCountryCode,
            ["redesignedContactTelephoneText"] = NativeBookingAccessibilityIds.RedesignedContactTelephoneText,
            ["resultsSort"] = NativeSearchAccessibilityIds.ResultsSort,
            ["resultsFilter"] = NativeSearchAccessibilityIds.ResultsFilter,
            ["resultsAirportRoute"] = NativeSearchAccessibilityIds.ResultsAirportRoute,
            ["resultsReady"] = NativeSearchAccessibilityIds.ResultsReady,
            ["flexibleDateCarousel"] = NativeSearchAccessibilityIds.FlexibleDateCarousel,
            ["flexibleDatePrefix"] = NativeSearchAccessibilityIds.FlexibleDatePrefix,
            ["priceSortOption"] = NativeSearchAccessibilityIds.PriceSortOption,
            ["recommendedSortOption"] = NativeSearchAccessibilityIds.RecommendedSortOption,
            ["departureTimeSortOption"] = NativeSearchAccessibilityIds.DepartureTimeSortOption,
            ["durationSortOption"] = NativeSearchAccessibilityIds.DurationSortOption,
            ["applySort"] = NativeSearchAccessibilityIds.ApplySort,
            ["filterOptionPrefix"] = NativeSearchAccessibilityIds.FilterOptionPrefix,
            ["applyFilters"] = NativeSearchAccessibilityIds.ApplyFilters,
            ["clearFilters"] = NativeSearchAccessibilityIds.ClearFilters,
            ["activeFilterSummary"] = NativeSearchAccessibilityIds.ActiveFilterSummary,
            ["fareSelectorCabinPrefix"] = NativeSearchAccessibilityIds.FareSelectorCabinPrefix,
            ["fareSelectorDismiss"] = NativeSearchAccessibilityIds.FareSelectorDismiss,
            ["redesignedFirstPassengerNameError"] = NativeBookingAccessibilityIds.RedesignedFirstPassengerNameError,
            ["fareSelectorFarePrefix"] = NativeSearchAccessibilityIds.FareSelectorFarePrefix,
            ["summaryBaggage"] = NativeSearchAccessibilityIds.SummaryBaggage,
            ["summaryItineraryPrefix"] = NativeSearchAccessibilityIds.SummaryItineraryPrefix,
            ["summaryFlightDetails0"] = NativeSearchAccessibilityIds.SummaryFlightDetails(0),
            ["flightLegPrefix"] = NativeSearchAccessibilityIds.FlightLegPrefix,
            ["flightConnectionPrefix"] = NativeSearchAccessibilityIds.FlightConnectionPrefix,
            ["resultFlightDetails0"] = NativeSearchAccessibilityIds.ResultFlightDetails(0),
            ["summaryFareTitle0"] = NativeSearchAccessibilityIds.SummaryFareTitle(0),
            ["summaryChange0"] = NativeSearchAccessibilityIds.SummaryChange(0),
            ["memberSelector"] = NativeBookingAccessibilityIds.MemberSelector,
            ["memberSelfOption"] = NativeBookingAccessibilityIds.MemberSelfOption,
            ["memberNotTravellingOption"] = NativeBookingAccessibilityIds.MemberNotTravellingOption,
            ["summaryUpgradeButton0"] = NativeSearchAccessibilityIds.SummaryUpgradeButton(0),
            ["summaryUpgradeTitle0"] = NativeSearchAccessibilityIds.SummaryUpgradeTitle(0),
            ["summaryUpgradeDifference0"] = NativeSearchAccessibilityIds.SummaryUpgradeDifference(0),
            ["summaryUpgradeTotal0"] = NativeSearchAccessibilityIds.SummaryUpgradeTotal(0),
            ["summaryError"] = NativeSearchAccessibilityIds.SummaryError,
            ["fareTitle0"] = NativeSearchAccessibilityIds.FareTitle(0),
            ["fareAppliedPromotion0"] = NativeSearchAccessibilityIds.FareAppliedPromotion(0),
            ["summaryBaggageAllowancePrefix"] = NativeSearchAccessibilityIds.SummaryBaggageAllowancePrefix,
            ["fareSelectorUpgradeCard"] = NativeSearchAccessibilityIds.FareSelectorUpgradeCard,
            ["fareSelectorUpgradeViewFares"] = NativeSearchAccessibilityIds.FareSelectorUpgradeViewFares,
            ["payWithPointsCard"] = NativeBookingAccessibilityIds.PayWithPointsCard,
            ["paymentSummaryCard"] = NativeBookingAccessibilityIds.PaymentSummaryCard,
            ["paymentFareConditionsLink"] = NativeBookingAccessibilityIds.PaymentFareConditionsLink,
            ["paymentGuidance"] = NativeBookingAccessibilityIds.PaymentGuidance,
            ["paymentBackButton"] = NativeBookingAccessibilityIds.PaymentBackButton,
            ["cardPresentationPage"] = NativeBookingAccessibilityIds.CardPresentationPage,
            ["cardPresentationHeading"] = NativeBookingAccessibilityIds.CardPresentationHeading,
            ["cardPresentationIntroduction"] = NativeBookingAccessibilityIds.CardPresentationIntroduction,
            ["cardPresentationDetails"] = NativeBookingAccessibilityIds.CardPresentationDetails,
            ["cardPresentationCancel"] = NativeBookingAccessibilityIds.CardPresentationCancel,
            ["cardPresentationAgree"] = NativeBookingAccessibilityIds.CardPresentationAgree,
            ["payWithPointsOptionSelector"] = NativeBookingAccessibilityIds.PayWithPointsOptionSelector,
            ["payWithPointsOptionPrefix"] = NativeBookingAccessibilityIds.PayWithPointsOptionPrefix,
            ["payWithPointsApplyButton"] = NativeBookingAccessibilityIds.PayWithPointsApplyButton,
            ["payWithPointsApplied"] = NativeBookingAccessibilityIds.PayWithPointsApplied,
            ["payWithPointsAmountToPay"] = NativeBookingAccessibilityIds.PayWithPointsAmountToPay,
            ["payWithPointsRemoveButton"] = NativeBookingAccessibilityIds.PayWithPointsRemoveButton,
            ["payWithPointsLoading"] = NativeBookingAccessibilityIds.PayWithPointsLoading,
            ["payWithPointsInsufficientBalance"] = NativeBookingAccessibilityIds.PayWithPointsInsufficientBalance,
            ["payWithPointsOptionsUnavailable"] = NativeBookingAccessibilityIds.PayWithPointsOptionsUnavailable,
            ["accountSettingsButton"] = LoyaltyPersonalDetailsPage.SettingsButton,
            ["accountSettingsScreen"] = LoyaltyPersonalDetailsPage.SettingsScreen,
            ["accountPersonalDetailsRow"] = LoyaltyPersonalDetailsPage.PersonalDetailsRow,
            ["accountMemberNameHeader"] = LoyaltyPersonalDetailsPage.MemberNameHeader,
            ["accountEditDetails"] = LoyaltyPersonalDetailsPage.EditDetails,
            ["accountDelete"] = LoyaltyPersonalDetailsPage.DeleteAccount,
        };

        JObject selectors = Assert.IsType<JObject>(contract["selectors"]);
        Assert.Equal(expectedSelectors.Count, selectors.Count);
        foreach ((string name, string value) in expectedSelectors)
        {
            Assert.Equal(value, selectors.Value<string>(name));
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SearchBookContract_MapsEveryScenarioToOneCategorisedXunitTest()
    {
        JObject contract = ReadContract();
        JArray scenarios = Assert.IsType<JArray>(contract["scenarios"]);
        Assert.True(File.Exists(CataloguePath), $"Mobile test catalogue not found: {CataloguePath}");
        string catalogue = File.ReadAllText(CataloguePath);
        string[] expectedIds =
        [
            "PANEL-TC01", "PANEL-TC02", "TC36", "TC37", "ANDROID-TC05", "ANDROID-TC06",
            "TC01", "TC02", "TC03", "TC04", "TC05", "TC06", "TC07", "TC08", "TC09", "TC10",
            "TC11", "TC12", "TC13", "TC14", "TC15", "TC16", "TC17", "TC18", "TC19", "TC20",
            "TC21", "TC22", "TC23", "TC24", "TC25", "TC26", "TC27", "TC28", "TC29", "TC30",
            "TC31", "TC32", "TC33", "TC34", "TC35", "TC38", "TC39", "TC40", "TC41", "TC42", "TC43", "TC44", "TC45", "TC46", "TC47",
            "TC48", "TC49", "TC50", "TC51", "TC52", "TC53", "TC54", "TC55", "TC56", "TC57",
            "LOYALTY-TC01", "LOYALTY-TC02", "LOYALTY-TC03", "LOYALTY-TC04",
            "FAULT-TC01", "FAULT-TC02", "FAULT-TC03", "FAULT-TC04", "FAULT-TC05", "FAULT-TC06", "FAULT-TC07", "FAULT-TC08", "FAULT-TC09", "FAULT-TC10", "FAULT-TC11", "FAULT-TC12", "ACCOUNT-TC01",
        ];
        string[] actualIds = scenarios
            .Select(scenario => scenario.Value<string>("id") ?? string.Empty)
            .ToArray();

        Assert.Equal(expectedIds, actualIds);
        Assert.Equal(actualIds.Length, actualIds.Distinct(StringComparer.Ordinal).Count());

        MethodInfo[] searchBookTests = typeof(MobileAutomationContractTests).Assembly
            .GetTypes()
            .Where(type =>
                type.Namespace?.EndsWith("Tests.SearchAndBook", StringComparison.Ordinal) == true ||
                type.Namespace?.EndsWith("Tests.Account", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Where(method => method.GetCustomAttribute<FactAttribute>() is not null)
            .ToArray();

        foreach (JToken scenario in scenarios)
        {
            string testMethod = RequiredValue(scenario, "testMethod");
            string category = RequiredValue(scenario, "category");
            Assert.False(RequiredValue(scenario, "outcome").IsNullOrWhiteSpace());
            string scenarioId = RequiredValue(scenario, "id");
            if (scenarioId.StartsWith("TC", StringComparison.Ordinal))
            {
                Assert.StartsWith($"SearchBook_{scenarioId}_", testMethod, StringComparison.Ordinal);
            }

            MethodInfo method = Assert.Single(searchBookTests, candidate =>
                candidate.Name.Equals(testMethod, StringComparison.Ordinal));
            Assert.True(HasTrait(method, "Category", category));
            bool panelReadiness = scenarioId.StartsWith("PANEL-", StringComparison.Ordinal) ||
                scenarioId.StartsWith("ANDROID-", StringComparison.Ordinal);
            if (panelReadiness)
            {
                // Preserve the separate readiness/pending packs merged in PR 2194.
                Assert.False(HasTrait(method, "Suite", "Regression"));
            }
            else
            {
                string suite = scenario.Value<string>("suite") ?? "Regression";
                Assert.True(HasTrait(method, "Suite", suite));
                if (suite.Equals("Regression", StringComparison.Ordinal) ||
                    suite.Equals("BackendRegression", StringComparison.Ordinal))
                {
                    string expectedRegressionArea = scenarioId.Equals("ACCOUNT-TC01", StringComparison.Ordinal)
                        ? "MyAccount"
                        : "SearchAndBook";
                    Assert.True(HasTrait(method, "RegressionArea", expectedRegressionArea));
                }
                else
                {
                    Assert.True(HasTrait(method, "Area", RequiredValue(scenario, "area")));
                }
            }
            Assert.Contains($"`{testMethod}`", catalogue, StringComparison.Ordinal);
            string[] platforms = scenario["platforms"]?.Values<string>().Select(value => value ?? string.Empty).ToArray()
                ?? [scenario.Value<string>("platform") ?? "iOS"];
            Assert.All(platforms, platform => Assert.True(HasTrait(method, "Platform", platform)));
        }

        Assert.Equal(scenarios.Count, searchBookTests.Count(method =>
            method.Name.StartsWith("AndroidSearchBook_TC", StringComparison.Ordinal) ||
            method.Name.StartsWith("SearchPanel_TC", StringComparison.Ordinal) ||
            method.Name.StartsWith("SearchBook_TC", StringComparison.Ordinal) ||
            method.Name.StartsWith("SearchBookFault_TC", StringComparison.Ordinal) ||
            method.Name.StartsWith("LoyaltySearchBook_TC", StringComparison.Ordinal) ||
            method.Name.StartsWith("LoyaltyAccount_TC", StringComparison.Ordinal)));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SearchBookSmokeE2E_EachScenarioOwnsAnIndependentTestClass()
    {
        Type[] scenarioClasses = typeof(MobileAutomationContractTests).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.EndsWith("Tests.SearchAndBook", StringComparison.Ordinal) == true)
            .Where(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Any(method => method.GetCustomAttribute<FactAttribute>() is not null &&
                    method.Name.StartsWith("SearchBook_TC0", StringComparison.Ordinal) &&
                    method.Name.Length > "SearchBook_TC0".Length &&
                    method.Name["SearchBook_TC0".Length] is >= '1' and <= '4'))
            .ToArray();

        Assert.Equal(4, scenarioClasses.Length);
        Assert.Equal(4, scenarioClasses.Distinct().Count());
        Assert.All(scenarioClasses, type =>
            Assert.Null(type.GetCustomAttribute<CollectionAttribute>()));
    }

    private static JObject ReadContract()
    {
        Assert.True(File.Exists(ContractPath), $"Mobile automation contract not found: {ContractPath}");
        return JObject.Parse(File.ReadAllText(ContractPath));
    }

    private static string RequiredValue(JToken token, string propertyName)
    {
        string value = token.Value<string>(propertyName) ?? string.Empty;
        Assert.False(value.IsNullOrWhiteSpace(), $"Contract property '{propertyName}' is required.");
        return value;
    }

    private static bool HasTrait(MethodInfo method, string name, string value)
        => method.CustomAttributes
            .Where(attribute => attribute.AttributeType == typeof(TraitAttribute))
            .Any(attribute =>
                attribute.ConstructorArguments.Count == 2 &&
                name.Equals(attribute.ConstructorArguments[0].Value as string, StringComparison.Ordinal) &&
                value.Equals(attribute.ConstructorArguments[1].Value as string, StringComparison.Ordinal));
}
