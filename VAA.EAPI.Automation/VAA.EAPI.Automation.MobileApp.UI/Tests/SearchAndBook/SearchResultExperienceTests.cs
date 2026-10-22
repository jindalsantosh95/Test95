using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class CascadingFilterRefinementTests : ResultsRefinementTestBase
{
    public CascadingFilterRefinementTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-CascadingFilters")]
    [Trait("Suite", "Sanity")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC19_FilterSelection_RecalculatesAvailabilityAcrossOtherGroups()
        => RunRefinementTest(
            page => page.AssertFilterOptionsCascadeAcrossGroups(),
            "Selecting an available filter recalculated options in another group and returned matching results.");
}

public sealed class VirginCabinNamingTests : ResultsRefinementTestBase
{
    public VirginCabinNamingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-CabinNames")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC20_VirginOperatingCarrierFlightCards_UseBroadCabinGroupNames()
        => RunRefinementTest(
            page => page.AssertVirginCabinNamesUseBroadGroups(),
            "Virgin cards used the Economy, Delight, Premium and Upper Class cabin groups.");
}

public sealed class PartnerCabinNamingTests : ResultsRefinementTestBase
{
    public PartnerCabinNamingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-CabinNames")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC21_OtherOperatingCarrierFlightCards_UseBackendBrandCabinNames()
        => RunRefinementTest(
            page => page.AssertPartnerCabinNamesUseBackendFareNames(),
            "Partner cards retained their backend fare or Brand cabin names.");
}

public sealed class VirginCabinUpgradeTests : ResultsRefinementTestBase
{
    public VirginCabinUpgradeTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-CabinUpgrade")]
    [Trait("Carrier", "Virgin")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC22_VirginFlightCabin_UpgradeViewFaresOpensNextAvailableCabin()
        => RunRefinementTest(
            page => page.AssertVirginCabinOffersUpgradeToNextAvailableCabin(),
            "The Virgin upgrade card moved the fare selector to the next available cabin.");
}
