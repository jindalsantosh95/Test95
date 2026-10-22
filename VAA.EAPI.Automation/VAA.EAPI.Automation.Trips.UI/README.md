# Trips UI Playwright automation

This project validates the anonymous My Trips “Add a trip” journey. It selects a current-week
one-way revenue PNR from the existing `MobileAppPnrs` Azure Table Storage helper, enters the
stored passenger details, and verifies that the application displays the same PNR after retrieval.

## Page Object architecture

```text
Tests → Flows → Pages → BasePage → Playwright session
```

- `Tests/` contains scenario setup and final business assertions only.
- `Flows/` coordinates complete multi-page journeys and session reporting.
- `Pages/` owns every Playwright API, selector, wait and page-state check.
- `Support/` owns test-data acquisition and non-browser helpers.

Functional tests and flows must not import Playwright or call browser/locator APIs directly. New
screens and reusable widgets are implemented as page/component objects inheriting `BasePage`.

Set `TRIPS_UI_BASE_URL` to the deployed application URL, ending in `/my-trips-dev` (with or without a trailing `/`), then run:

```pwsh
dotnet test VAA.EAPI.Automation\VAA.EAPI.Automation.Trips.UI\VAA.EAPI.Automation.Trips.UI.csproj -c Release --filter "Category=Trips-UI-Regression"
```

BrowserStack uses the shared Playwright options. Set `SAB_RUN_MODE=BrowserStack` and the usual
`BROWSERSTACK_USERNAME`, `BROWSERSTACK_ACCESS_KEY`, and `SAB_BS_*` variables. Storage credentials
remain external Key Vault/environment configuration and are never committed.

Use the project scripts for setup and regression runs:

```pwsh
.\Scripts\Setup-TripsUi.ps1
.\Scripts\Run-TripsUiRegression.ps1 -BaseUrl "https://vaa-test1.virginatlantic.com/my-trips-dev"
```

For BrowserStack, add `-RunMode BrowserStack`; use `-BrowserStackLocal` when the deployed
frontend is reachable only through the BrowserStack Local tunnel.
