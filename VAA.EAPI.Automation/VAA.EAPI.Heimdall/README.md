# Heimdall synthetic monitoring

Heimdall is the scheduled Windows-service runner for production and environment health checks.
Tests are selected by assembly and xUnit traits, so a monitor must use the established category
and tag values configured for its agent.

## Search & Book production UI monitoring

The `VAA.EAPI.Automation.Heimdall.UI` companion project contains two Playwright guest journeys:

- Upper Class selects an available included seat and reaches an interactable hosted card form.
- Economy Light continues without adding a seat, verifies PayPal is available, and reaches an
  interactable hosted card form.

Both journeys stop at HPP by setting `StopAtHostedPaymentPage`; they never enter card details,
submit payment, or create an order. Existing API-led Heimdall checks also use Playwright for their
direct HPP validation and retain their previous non-production payment behaviour.

Configure the production Heimdall service account with:

```text
env=prod
SAB_BASE_URL=https://www.virginatlantic.com
SAB_RUN_MODE=Local
SAB_BROWSER=chromium
SAB_BROWSER_CHANNEL=chrome
SAB_HEADLESS=true
HPP_PAGE_ALLOWED_ORIGINS=<approved production HPP origin>
```

`env=prod` is mandatory for the production service account. Existing API-led GraphQL and BFF
checks use it to stop after HPP validation instead of running their non-production payment paths.
The exact HPP origin must come from the environment's approved service configuration; do not use
a wildcard or infer it from a returned payment link.

Using the installed Chrome channel avoids dependence on a separately downloaded Playwright browser
revision. If the channel is omitted, install the matching browser as the service account after the
solution has been built:

```powershell
pwsh .\VAA.EAPI.Automation.SearchAndBook.UI\bin\Release\net10.0\playwright.ps1 install chromium
```

The runner assembly expression must include `VAA.EAPI.Automation.Heimdall.UI`, and its test filter
must include `Category=HealthCheck` and `Tag=DotCom`. Sanitised Playwright diagnostics are written
beneath `TestResults/playwright`; retain that directory with the Heimdall failure evidence. Query
and fragment values are removed from recorded URLs, and tracing stays disabled for HPP journeys.

Run the scoped tests manually with TRX output before enabling the schedule:

```powershell
dotnet test .\VAA.EAPI.Automation.Heimdall.UI\VAA.EAPI.Automation.Heimdall.UI.csproj -c Release `
  --filter "FullyQualifiedName~ProductionGuestUi" `
  --results-directory TestResults\heimdall-production-ui `
  --logger "trx;LogFileName=heimdall-production-ui.trx"
```
