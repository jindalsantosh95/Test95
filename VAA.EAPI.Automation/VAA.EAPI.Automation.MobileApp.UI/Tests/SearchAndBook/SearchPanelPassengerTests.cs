using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SearchPanelPassengerTests : SearchPanelTestBase
{
    public SearchPanelPassengerTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchPanel-Readiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "iOS")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "AppContractRequired")]
    [Trait("Suite", "Readiness")]
    [Trait("Area", "SearchAndBook")]
    public void SearchBook_TC37_PassengerCounts_ConfirmPersistsAndCancelDiscardsDraft()
        => RunPanelScenario(page =>
    {
        page.SetOneAdult();
        page.OpenPassengers();
        Assert.Equal(1, page.PassengerCount("ADULTS"));
        page.SetPassengerCount("ADULTS", 2);
        page.SetPassengerCount("TEENS", 1);
        page.SetPassengerCount("CHILDREN", 1);
        page.SetPassengerCount("INFANTS", 1);
        page.ConfirmPassengers();
        Assert.True(page.HasPassengerTotal(5));

        page.OpenPassengers();
        Assert.Equal(2, page.PassengerCount("ADULTS"));
        Assert.Equal(1, page.PassengerCount("TEENS"));
        Assert.Equal(1, page.PassengerCount("CHILDREN"));
        Assert.Equal(1, page.PassengerCount("INFANTS"));
        page.SetPassengerCount("ADULTS", 3);
        page.ClosePassengers();
        Assert.True(page.HasPassengerTotal(5));
        page.OpenPassengers();
        Assert.Equal(2, page.PassengerCount("ADULTS"));
        page.ClosePassengers();
        page.SetOneAdult();
    });
}
