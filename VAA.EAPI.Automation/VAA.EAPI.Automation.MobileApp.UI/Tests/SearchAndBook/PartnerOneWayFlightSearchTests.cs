using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PartnerOneWayFlightSearchTests : OneWayFlightSearchTestBase
{
    public PartnerOneWayFlightSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "OneWay")]
    [Trait("Carrier", "Partner")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC02_MixedPassengerOneWay_SelectsNonVirginFlight()
        => RunMixedPassengerOneWay(virginFlight: false, completeCheckout: false);
}
