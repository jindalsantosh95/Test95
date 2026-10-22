using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests;

internal static class TestOptions
{
    private const string BaseUrlVariable = "IN_FLIGHT_APPLICATIONS_UI_BASE_URL";

    public static PlaywrightBrowserOptions FromEnvironment()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlVariable);
        Skip.If(baseUrl.IsNullOrWhiteSpace(), $"{BaseUrlVariable} is not configured.");
        return PlaywrightBrowserOptions.FromEnvironment(
            baseUrl ?? throw new InvalidOperationException($"{BaseUrlVariable} is not configured."));
    }

    public static void RequireOtpJourneys()
    {
        string? enabled = Environment.GetEnvironmentVariable("IN_FLIGHT_APPLICATIONS_RUN_OTP_TESTS");
        Skip.IfNot(
            enabled.Is("true"),
            "Set IN_FLIGHT_APPLICATIONS_RUN_OTP_TESTS=true to opt into stateful OTP journeys.");
    }

    public static PortalAccountCredentials RequireAccount(string emailVariable)
    {
        string? enabled = Environment.GetEnvironmentVariable("IN_FLIGHT_APPLICATIONS_RUN_ACCOUNT_TESTS");
        Skip.IfNot(
            enabled.Is("true"),
            "Set IN_FLIGHT_APPLICATIONS_RUN_ACCOUNT_TESTS=true to opt into shared-account journeys.");

        string? email = Environment.GetEnvironmentVariable(emailVariable);
        string? password = Environment.GetEnvironmentVariable("E2E_LOGIN_PASSWORD");
        Skip.If(email.IsNullOrWhiteSpace(), $"{emailVariable} is not configured.");
        Skip.If(password.IsNullOrWhiteSpace(), "E2E_LOGIN_PASSWORD is not configured.");
        return new PortalAccountCredentials(email!, password!);
    }
}

public sealed record PortalAccountCredentials(string Email, string Password);
