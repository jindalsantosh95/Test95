using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class AndroidSearchPromoTests : AndroidSearchPanelTestBase
{
    public AndroidSearchPromoTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-AndroidReadiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "AppContractRequired")]
    public void AndroidSearchBook_TC05_DiscountCode_ValidatesAppliesAndClearsPanelEntry()
        => RunPanelScenario(page =>
    {
        page.OpenPromo();
        page.ClearPromo();
        page.OpenPromo();
        Assert.False(page.CanApplyPromo);
        page.EnterPromo("AB");
        page.WaitForPromoValidity(false);
        Assert.False(page.CanApplyPromo);
        page.EnterPromo("QA123");
        page.WaitForPromoValidity(true);
        Assert.True(page.CanApplyPromo);
        page.ApplyPromo();
        Assert.True(page.HasPromo("QA123"));

        page.OpenPromo();
        Assert.Equal("QA123", page.PromoValue);
        page.EnterPromo("QA456");
        page.ClosePromo();
        page.OpenPromo();
        Assert.Equal("QA123", page.PromoValue);
        page.ClearPromo();
        Assert.False(page.HasPromo("QA123"));
        page.OpenPromo();
        Assert.Equal(string.Empty, page.PromoValue);
        Assert.False(page.CanApplyPromo);
        page.ClosePromo();
    });
}
