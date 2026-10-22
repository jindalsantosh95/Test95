using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class DiscountCodeLifecycleTests : DiscountCodePanelTestBase
{
    public DiscountCodeLifecycleTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-DiscountCode")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC32_DiscountCode_CancelPreservesAppliedCodeAndClearRemovesIt()
        => RunDiscountCodeScenario(page =>
        {
            string emptyPanelLabel = page.ReadDiscountCodePanelLabel();
            page.ApplyDiscountCode("QA123");
            string appliedPanelLabel = page.ReadDiscountCodePanelLabel();
            Assert.NotEqual(emptyPanelLabel, appliedPanelLabel);
            page.OpenDiscountCodeEditor();
            Assert.Equal("QA123", page.ReadDiscountCodeDraft());
            page.EnterDiscountCodeDraft("QA456", expectedApplyEnabled: true);
            page.CancelDiscountCodeDraft();
            Assert.Equal(appliedPanelLabel, page.ReadDiscountCodePanelLabel());

            page.OpenDiscountCodeEditor();
            Assert.Equal("QA123", page.ReadDiscountCodeDraft());
            page.EnterDiscountCodeDraft("QA789", expectedApplyEnabled: true);
            page.ApplyDiscountCodeDraft();
            Assert.Contains("QA789", page.ReadDiscountCodePanelLabel(), StringComparison.Ordinal);
            Assert.DoesNotContain("QA123", page.ReadDiscountCodePanelLabel(), StringComparison.Ordinal);
            page.OpenDiscountCodeEditor();
            Assert.Equal("QA789", page.ReadDiscountCodeDraft());
            page.ClearDiscountCode();
            Assert.Equal(emptyPanelLabel, page.ReadDiscountCodePanelLabel());
            page.OpenDiscountCodeEditor();
            Assert.False(page.IsDiscountCodeApplyEnabled());
            Assert.DoesNotContain("QA789", page.ReadDiscountCodeDraft(), StringComparison.Ordinal);
            page.CancelDiscountCodeDraft();
        });
}
