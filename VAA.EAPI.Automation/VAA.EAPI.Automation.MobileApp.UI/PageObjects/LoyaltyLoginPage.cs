using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class LoyaltyLoginPage : NativeAppPage
{
    private readonly PrivacyBannerPage _privacyBanner;
    private const string AccountTab = "person";
    private const string GuestSignIn = "guestLoginButton";
    private const string LandingSignIn = "Sign in";
    private const string Email = "EmailText";
    private const string Password = "PasswordText";
    private const string Submit = "loginButtonTitle";
    private const string Greeting = "greeting";
    private const string Points = "virginPointsValue";
    private const string Settings = "settingsBarButton";
    private const string Logout = "logoutRow";
    private const string ConfirmLogout = "Sign out";
    // Temporary exact English accessible-name fallback, shared with the existing login outcome.
    // Replace with an app-owned Account error identifier when the app contract supplies one.
    private const string AccountLoadError = "Something went a little off course";
    private static readonly TimeSpan LoginTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromMilliseconds(350);

    public LoyaltyLoginPage(AppiumDriver driver)
        : base(driver, LoginTimeout)
    {
        _privacyBanner = new PrivacyBannerPage(driver);
    }

    public LoyaltyLoginResult Login(string identifier, string password, int maximumAttempts = 3)
    {
        if (identifier.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A non-empty loyalty identifier and password are required.");
        }

        if (maximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAttempts),
                maximumAttempts,
                "The maximum number of login attempts must be greater than zero.");
        }

        OpenLogin();
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            EnterCredentials(identifier, password);
            LoginOutcome outcome = WaitForOutcome();

            if (outcome == LoginOutcome.MemberAccount)
            {
                return ReadMemberResult();
            }

            if (outcome == LoginOutcome.InvalidCredentials)
            {
                throw new InvalidOperationException(
                    "The loyalty account was rejected as invalid. The test did not retry a permanent credential failure.");
            }

            if (attempt == maximumAttempts)
            {
                throw new WebDriverTimeoutException(
                    $"Loyalty login did not reach the member account after {maximumAttempts} attempts. " +
                    $"Last outcome: {outcome}. Inspect failure evidence for the app/backend response.");
            }

            HealingPolicy.ExecuteRecovery(
                MobileHealingType.TransientStateRetry,
                "Recover loyalty login after a recognised transient outcome",
                attempt,
                maximumAttempts - 1,
                $"The app reported the bounded transient login outcome '{outcome}'. " +
                "Invalid credentials and permanent business failures are never retried.",
                () => RecoverForRetry(outcome));
        }

        throw new InvalidOperationException("The login retry loop ended unexpectedly.");
    }

    public void EnsureGuestSession()
    {
        _privacyBanner.DismissIfPresent();
        IWebElement? accountTab = null;
        for (int navigationAttempt = 0; navigationAttempt < 5 && accountTab is null; navigationAttempt++)
        {
            accountTab = TryWaitForElementByKey(AccountTab, TimeSpan.FromSeconds(2));
            if (accountTab is null)
            {
                Driver.Navigate().Back();
            }
        }

        if (accountTab is null)
        {
            throw new InvalidOperationException(
                "The Account tab was not available while verifying the guest-session precondition.");
        }

        LoginEntryState entryState = OpenAccountEntry();
        if (entryState == LoginEntryState.MemberAccount)
        {
            LogoutFromMemberAccount();
            entryState = WaitForLoginEntryState();
        }

        if (entryState == LoginEntryState.MemberAccount)
        {
            throw new InvalidOperationException(
                "The guest-session precondition failed because a member remained signed in.");
        }
    }

    private void OpenLogin()
    {
        _privacyBanner.DismissIfPresent();
        OpenLoginFormFromEntryState(OpenAccountEntry());
    }

    private LoginEntryState OpenAccountEntry()
    {
        LoginEntryState state = default;
        NavigateToAccount(
            () =>
            {
                IWebElement accountTab = WaitForElementByKey(AccountTab);
                new WebDriverWait(Driver, TimeSpan.FromSeconds(10)).Until(_ =>
                    accountTab.Displayed && accountTab.Enabled);
                accountTab.Click();
            },
            () => state = WaitForLoginEntryState(TimeSpan.FromSeconds(5)),
            HealingPolicy);
        return state;
    }

    internal static void NavigateToAccount(Action tapAccount, Action waitForEntryState, MobileHealingPolicy healingPolicy)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            // Keep lookup/tap failures outside the state-timeout catch. Only an unobserved
            // destination permits a re-tap; no login, logout or business action is replayed.
            if (attempt == 1)
            {
                tapAccount();
            }
            else
            {
                healingPolicy.ExecuteRecovery(
                    MobileHealingType.TransientStateRetry,
                    "Reopen Account after navigation did not settle",
                    attempt - 1,
                    2,
                    "No member, guest, landing or login state appeared after the previous Account tap.",
                    tapAccount);
            }

            try
            {
                waitForEntryState();
                return;
            }
            catch (AccountEntryReadinessTimeoutException) when (attempt < 3)
            {
                // A freshly dismissed privacy overlay can leave the tab bar visible before it
                // accepts interaction. Retry only when no observable Account state appeared.
            }
        }
    }

    private void OpenLoginFormFromCurrentEntryState(TimeSpan? stateTimeout = null)
        => OpenLoginFormFromEntryState(WaitForLoginEntryState(stateTimeout));

    private void OpenLoginFormFromEntryState(LoginEntryState entryState)
    {
        if (entryState == LoginEntryState.MemberAccount)
        {
            LogoutMember();
            entryState = WaitForLoginEntryState();
        }

        if (entryState == LoginEntryState.LoginForm)
        {
            return;
        }

        string signInId = entryState == LoginEntryState.AccountGuest
            ? GuestSignIn
            : LandingSignIn;
        IWebElement signIn = WaitForElementByKey(signInId, TimeSpan.FromSeconds(5));
        signIn.Click();
        WaitForElementByKey(Email);
    }

    private LoginEntryState WaitForLoginEntryState(TimeSpan? timeout = null)
    {
        try
        {
            return WaitForAccountReadiness(() =>
            {
                _privacyBanner.DismissVisibleActionIfPresent();

                if (IsDisplayedByKey(AccountLoadError))
                {
                    throw new InvalidOperationException(
                        "Account displayed its load-error state before the guest/member precondition could be verified. " +
                        "No navigation or sign-in retry was attempted for this error; inspect failure evidence.");
                }

                if (IsMemberAccount())
                {
                    return new LoginEntryStateResult(LoginEntryState.MemberAccount);
                }

                if (IsDisplayedByKey(Email))
                {
                    return new LoginEntryStateResult(LoginEntryState.LoginForm);
                }

                if (IsDisplayedByKey(GuestSignIn))
                {
                    return new LoginEntryStateResult(LoginEntryState.AccountGuest);
                }

                return IsDisplayedByKey(LandingSignIn)
                    ? new LoginEntryStateResult(LoginEntryState.Landing)
                    : null;
            }, timeout ?? TimeSpan.FromSeconds(20)).State;
        }
        catch (AccountEntryReadinessTimeoutException exception)
        {
            throw new AccountEntryReadinessTimeoutException(
                $"Account did not settle into a member, guest, landing or login state. " +
                $"Visible controls: {DescribeVisibleControls()}",
                exception);
        }
    }

    internal static T WaitForAccountReadiness<T>(Func<T?> poll, TimeSpan timeout) where T : class
    {
        DefaultWait<Func<T?>> wait = new(poll)
        {
            Timeout = timeout,
            PollingInterval = TimeSpan.FromMilliseconds(400)
        };
        bool commandTimedOut = false;
        try
        {
            return wait.Until(probe =>
            {
                try
                {
                    return probe();
                }
                catch (WebDriverTimeoutException)
                {
                    // A lookup/privacy action timeout is not evidence of an absent destination.
                    commandTimedOut = true;
                    throw;
                }
            });
        }
        catch (WebDriverTimeoutException exception) when (!commandTimedOut)
        {
            throw new AccountEntryReadinessTimeoutException(
                "Account readiness polling expired without observing a destination.", exception);
        }
    }

    public void LogoutMember()
    {
        IWebElement? accountTab = null;
        for (int navigationAttempt = 0; navigationAttempt < 5 && accountTab is null; navigationAttempt++)
        {
            accountTab = TryWaitForElementByKey(AccountTab, TimeSpan.FromSeconds(2));
            if (accountTab is null)
            {
                Driver.Navigate().Back();
            }
        }

        if (accountTab is null)
        {
            throw new InvalidOperationException(
                "The Account tab was not available before member logout after bounded back navigation.");
        }
        accountTab.Click();

        WebDriverWait memberWait = new(Driver, TimeSpan.FromSeconds(15))
        {
            PollingInterval = TimeSpan.FromMilliseconds(400)
        };
        memberWait.Until(_ => IsMemberAccount());

        LogoutFromMemberAccount();
    }

    private void LogoutFromMemberAccount()
    {
        TapByKey(Settings);
        TapByKey(Logout);
        TapByKey(ConfirmLogout);
        LoginEntryState signedOutState = WaitForLoginEntryState();
        if (signedOutState == LoginEntryState.MemberAccount)
        {
            throw new InvalidOperationException("Member Account remained visible after logout.");
        }
    }

    private void EnterCredentials(string identifier, string password)
    {
        EnterSecureText(Email, identifier);
        EnterSecureText(Password, password);
        TapByKey(Submit);
    }

    private void EnterSecureText(string accessibilityId, string value)
    {
        IWebElement field = WaitForElementByKey(accessibilityId);
        field.Click();
        field.Clear();
        field.SendKeys(value);
    }

    private LoginOutcome WaitForOutcome()
    {
        WebDriverWait wait = new(Driver, LoginTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(400)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        try
        {
            return wait.Until(_ =>
            {
                HandleBlockingSystemAlert();

                if (IsMemberAccount())
                {
                    return new LoginOutcomeResult(LoginOutcome.MemberAccount);
                }

                string source = Driver.PageSource;
                if (source.Contains("The details you have entered are incorrect", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginOutcomeResult(LoginOutcome.InvalidCredentials);
                }

                if (source.Contains(AccountLoadError, StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginOutcomeResult(LoginOutcome.TransientError);
                }

                // Login can complete on Explore/Trips. Return to Account and verify member state.
                IWebElement? accountTab = TryWaitForElementByKey(AccountTab, ProbeTimeout);
                if (accountTab is not null && accountTab.Displayed)
                {
                    accountTab.Click();
                    return IsMemberAccount()
                        ? new LoginOutcomeResult(LoginOutcome.MemberAccount)
                        : null;
                }

                return null;
            }).Outcome;
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "Timed out waiting for an explicit loyalty-login outcome: member account, invalid credentials, or transient app error.",
                exception);
        }
    }

    private void RecoverForRetry(LoginOutcome outcome)
    {
        if (outcome == LoginOutcome.TransientError)
        {
            IWebElement? ok = TryWaitForElementByKey("OK", TimeSpan.FromSeconds(2))
                ?? TryWaitForElementByKey("Ok", TimeSpan.FromSeconds(1));
            ok?.Click();
        }

        if (!IsVisibleByKey(Email, TimeSpan.FromSeconds(3)))
        {
            IWebElement? accountTab = TryWaitForElementByKey(AccountTab, ProbeTimeout);
            accountTab?.Click();
            OpenLoginFormFromCurrentEntryState();
        }

        WaitForElementByKey(Email);
    }

    private bool IsMemberAccount() =>
        IsDisplayedByKey(Points) ||
        IsDisplayedByKey("membershipNumberElement") ||
        IsDisplayedByKey("memberNameLabel") ||
        IsDisplayedByKey("activityCard");

    private bool IsDisplayedByKey(string accessibilityId)
    {
        try
        {
            return Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .Any(element => element.Displayed);
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private LoyaltyLoginResult ReadMemberResult()
    {
        IWebElement? greeting = TryWaitForElementByKey(Greeting, TimeSpan.FromSeconds(5));
        IWebElement? points = TryWaitForElementByKey(Points, TimeSpan.FromSeconds(2));
        return new LoyaltyLoginResult(ReadAccessibleText(greeting), ReadAccessibleText(points));
    }

    private static string ReadAccessibleText(IWebElement? element)
    {
        if (element is null)
        {
            return string.Empty;
        }

        string text = element.Text;
        return !text.IsNullOrWhiteSpace()
            ? text
            : element.GetAttribute("label") ?? element.GetAttribute("value") ?? string.Empty;
    }

    private void HandleBlockingSystemAlert()
    {
        try
        {
            IAlert alert = Driver.SwitchTo().Alert();
            string text = alert.Text ?? string.Empty;
            if (text.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                alert.Dismiss();
            }
            else if (text.Contains("notification", StringComparison.OrdinalIgnoreCase) ||
                     text.Contains("send you", StringComparison.OrdinalIgnoreCase))
            {
                alert.Accept();
            }
        }
        catch (NoAlertPresentException)
        {
            // Polling the explicit UI outcomes remains the primary synchronisation mechanism.
        }
        catch (WebDriverException)
        {
            // Some remote Appium providers do not expose the alert API; normal outcome polling continues.
        }
    }

    private string DescribeVisibleControls()
    {
        try
        {
            XDocument source = XDocument.Parse(Driver.PageSource);
            string[] controls = source.Descendants()
                .Where(element =>
                    element.Attribute("visible")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                .SelectMany(element => new[]
                {
                    element.Attribute("name")?.Value,
                    element.Attribute("label")?.Value,
                })
                .Where(value => !value.IsNullOrWhiteSpace())
                .Distinct(StringComparer.Ordinal)
                .Take(30)
                .ToArray()!;
            return controls.Length == 0 ? "none exposed" : string.Join(" | ", controls);
        }
        catch (Exception exception)
        {
            return $"unavailable ({exception.GetType().Name})";
        }
    }

    private enum LoginOutcome
    {
        MemberAccount,
        InvalidCredentials,
        TransientError
    }

    private enum LoginEntryState
    {
        MemberAccount,
        AccountGuest,
        Landing,
        LoginForm
    }

    private sealed record LoginOutcomeResult(LoginOutcome Outcome);

    private sealed record LoginEntryStateResult(LoginEntryState State);
}

public sealed record LoyaltyLoginResult(string Greeting, string PointsText);
