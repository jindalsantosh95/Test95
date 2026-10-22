using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class ConnectionPage : BasePage
{
    private ILocator Heading => Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
    {
        Name = "You're connected"
    });

    public ConnectionPage(IPage page) : base(page)
    {
    }

    public async Task<JourneyResult> WaitUntilConnectedAsync()
    {
        await Heading.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000
        });
        return new JourneyResult(Page.Url, await Heading.InnerTextAsync());
    }
}

public sealed record JourneyResult(string Url, string Heading);
