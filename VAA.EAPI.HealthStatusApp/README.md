# VAA.EAPI.HealthStatusApp

Blazor Server web application for operational monitoring dashboards and tools.

## Key Pages
- `/deploymentstatus` - deployment queue/status dashboard, including queue counts and the most recent incomplete build timestamp from Insights.
- `/azure-app-settings` - step-by-step Azure Site / Function App settings explorer (subscription -> resource group -> site / function app), with case-insensitive sorting and filtering.
  - Shows a **Recent Alerts** table at the top of the page using `{HEALTH_CHECK_URL}/appSettings/diffalerts` (whether `/api` is included depends on how `HEALTH_CHECK_URL` is configured).
  - The alerts section auto-hides when no alerts are returned.
  - Alerts list is capped to `20vh` and scrolls when there are many rows.
  - Dropdown selectors now show an inline loading spinner while each dependent dataset is being fetched.
- `/service-bus-information` - Azure Service Bus explorer using the same Insights-backed subscription and resource group selectors, then `{HEALTH_CHECK_URL}/azure/subscriptions/{subscriptionId}/resourcegroups/{resourceGroup}/servicebus` to show namespaces, topics, subscriptions, and queue depths.

## Run locally
```bash
dotnet restore VAA.EAPI.HealthStatusApp/VAA.EAPI.HealthStatusApp.sln
dotnet build VAA.EAPI.HealthStatusApp/VAA.EAPI.HealthStatusApp.sln -c Release
```

## Required configuration
Set the following settings for Insights API integration:
- `HEALTH_CHECK_URL`
- `HEALTH_CHECK_SUBSCRIPTION_KEY`
- `HEALTH_CHECK_SUBSCRIPTION_KEY_TEST` (used in existing Insights connector paths)

## Authentication failure handling
Invalid OpenID Connect callback requests to `/signin-oidc` return a plain `400 Bad Request` response. Production unhandled errors return a minimal plain `500` response, and the operational navigation is rendered only for users who satisfy the `RequireSpecificGroup` policy.
