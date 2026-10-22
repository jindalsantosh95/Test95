using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class MixedCarrierReturnMultiPassengerFlightSearchTests : ReturnMultiPassengerFlightSearchTestBase
{
    public MixedCarrierReturnMultiPassengerFlightSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Return")]
    [Trait("Passengers", "MultiPassenger")]
    [Trait("Carrier", "Mixed")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC04_TwoAdultsAndChildReturn_SelectsVirginOutboundAndPartnerInbound()
        => RunReturn(virginOutbound: true, virginInbound: false, completeCheckout: false);
}
