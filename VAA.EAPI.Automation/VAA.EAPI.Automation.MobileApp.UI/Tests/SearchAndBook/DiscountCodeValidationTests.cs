using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class DiscountCodeValidationTests : DiscountCodePanelTestBase
{
    public DiscountCodeValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-DiscountCode")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC31_DiscountCode_RejectsInvalidDraftsAndTrimsAppliedCode()
        => RunDiscountCodeScenario(page =>
        {
            page.OpenDiscountCodeEditor();
            Assert.False(page.IsDiscountCodeApplyEnabled());
            foreach (string invalidCode in new[] { "AB", "QA!", "Q A" })
            {
                page.EnterDiscountCodeDraft(invalidCode, expectedApplyEnabled: false);
                Assert.False(page.IsDiscountCodeApplyEnabled());
            }

            page.EnterDiscountCodeDraft("QA1", expectedApplyEnabled: true);
            Assert.True(page.IsDiscountCodeApplyEnabled());
            page.EnterDiscountCodeDraft(" QA123 ", expectedApplyEnabled: true);
            page.ApplyDiscountCodeDraft();
            Assert.Contains("QA123", page.ReadDiscountCodePanelLabel(), StringComparison.Ordinal);
            page.OpenDiscountCodeEditor();
            Assert.Equal("QA123", page.ReadDiscountCodeDraft());
            page.ClearDiscountCode();
        });
}
