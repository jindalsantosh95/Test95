# Search & Book production UI monitors

This project contains the Playwright production journeys run by the Heimdall Windows service.
They validate the guest Upper Class and Economy Light paths through seat handling and stop once
the hosted payment page is ready. They do not enter payment details or create an order.

## Enable on a Heimdall agent

Deploy the TestRunner service publish artifact, ensure Google Chrome is installed on the agent,
and set:

```text
env=prod
SAB_BASE_URL=https://www.virginatlantic.com
SAB_RUN_MODE=Local
SAB_BROWSER=chromium
SAB_BROWSER_CHANNEL=chrome
SAB_HEADLESS=true
HPP_PAGE_ALLOWED_ORIGINS=<approved production HPP origin>
Configuration:TestRun:Assemblies=VAA\.EAPI\.Automation\.Heimdall(\.UI)?
Configuration:TestRun:Filter=Category=HealthCheck&Tag=DotCom
```

`env=prod` is a mandatory safety control for this combined production monitor configuration.
The existing API-led GraphQL and BFF checks use it to validate HPP without entering their
non-production payment-submission branches.

Direct HPP access is restricted to the known production payment origin. If a controlled HPP
migration introduces a different page origin, update `HPP_PAGE_ALLOWED_ORIGINS` with the approved
exact HTTPS origin only after verifying ownership; wildcards are not accepted. This setting is
mandatory for the API-led GraphQL/BFF monitors included by the combined assembly filter.

`SAB_BROWSER_CHANNEL=chrome` deliberately uses the installed Google Chrome channel. To use
Playwright's bundled Chromium instead, omit `SAB_BROWSER_CHANNEL` and install Chromium from the
published TestRunner directory with `playwright.ps1 install chromium`.

Keep the agent's existing `Configuration:TestRun:TimerInterval`, Application Insights connection
string, and service identity/network configuration. Restart the Windows service after changing
environment variables. The startup log must show the expected assemblies and a non-zero number
of runnable tests; zero assemblies or zero runnable tests is emitted as an error rather than a
healthy-looking empty run.

## Failure reporting

An assertion or journey exception is logged as `FailedTestException` at error level and therefore
flows through the TestRunner's existing Application Insights and Windows Event Log providers.
xUnit runner failures and zero-test misconfiguration are also logged at error level. A failed UI
journey retains its sanitised screenshot and network diagnostics under the agent's artifact
directory; successful journey artifacts are removed. Playwright tracing remains disabled because
an HPP trace can contain payment reference identifiers in navigation and request URLs.

Before enabling an L2 alert, deliberately run once with an invalid `SAB_BASE_URL` and confirm the
error event reaches the expected Application Insights alert/action group. Restore the production
URL and confirm both tests pass. This proves the telemetry route as well as the journey itself.
