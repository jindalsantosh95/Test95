using Microsoft.Playwright;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

/// <summary>
/// Shared Playwright page-object boundary for Search &amp; Book UI automation.
/// Browser primitives stay protected so tests and journey flows interact through
/// intent-based page methods rather than selectors or Playwright APIs.
/// </summary>
public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }
}
