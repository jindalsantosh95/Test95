using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Handlers;

public sealed class AppEntryHandler
{
    private readonly OnboardingPage _onboarding;
    private readonly PrivacyBannerPage _privacyBanner;
    private readonly WelcomePage _welcome;
    private readonly LoyaltyLoginPage _loyaltyLogin;

    public AppEntryHandler(AppiumDriver driver)
    {
        _onboarding = new OnboardingPage(driver);
        _privacyBanner = new PrivacyBannerPage(driver);
        _welcome = new WelcomePage(driver);
        _loyaltyLogin = new LoyaltyLoginPage(driver);
    }

    public void EnterAsGuest()
    {
        for (int transition = 0; transition < 5; transition++)
        {
            AppEntryState state = _welcome.WaitForNextEntryState(
                TimeSpan.FromSeconds(30),
                _privacyBanner.DismissVisibleActionIfPresent);
            switch (state)
            {
                case AppEntryState.NativeSearch:
                    _loyaltyLogin.EnsureGuestSession();
                    return;

                case AppEntryState.Onboarding:
                    _onboarding.CompleteIfPresent();
                    continue;

                case AppEntryState.GuestActionCompleted:
                case AppEntryState.PrivacyActionCompleted:
                    continue;

                case AppEntryState.PrimaryNavigation:
                    _loyaltyLogin.EnsureGuestSession();
                    return;
            }
        }

        throw new InvalidOperationException(
            "The app entry flow exceeded five native state transitions before becoming ready.");
    }
}
