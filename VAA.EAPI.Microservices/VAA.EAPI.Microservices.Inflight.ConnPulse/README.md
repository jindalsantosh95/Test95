# VAA.EAPI.Microservices.Inflight.ConnPulse

Azure Functions microservice that runs scheduled Starlink connectivity heartbeat checks.

## Triggers
- `HealthCheckTimer`: timer-triggered function that evaluates captive portal reachability and sends Starlink heartbeat payloads every minute.
- Deployment app settings disable `HealthCheckTimer` for non-production environments (`AzureWebJobs.HealthCheckTimer.Disabled=true`) and only enable it in production.

## Local development
1. Restore/build:
   - `dotnet restore VAA.EAPI.Microservices.sln`
   - `dotnet build VAA.EAPI.Microservices.sln -c Release`
2. Run Functions host from this project folder.
3. Heartbeat calls to Starlink include retry handling for transient transport errors (for example connection reset by peer) with up to 3 attempts.
4. Captive portal, GraphQL, and inflight connectivity probe requests run through `IHttpConnector`, including HEAD-to-GET fallback behavior for endpoints that do not support HEAD requests. GraphQL checks use the MyAccount API Management heartbeat routes.
5. The captive portal probe calls `CaptivePortalAsyncCheckUrl` and requires the `X-Backend` response header to match `CaptivePortalExpectedBackendHeaderValue` so the heartbeat confirms it has reached the expected portal app.

## Required configuration
- `StarlinkAuthenticationUrl`
- `StarlinkAPIClientId`
- `StarlinkClientSecret`
- `StarlinkSandboxUrl`
- `CaptivePortalAsyncCheckUrl` (set to `https://onboard.virginatlantic.com/?debug=<debug-token>` )
- `CaptivePortalExpectedBackendHeaderValue`
- `InflightConnUrl`
- `InflightConnectivityHealth` (Key Vault reference to the APIM subscription key used only for the connectivity probe)
- `GraphQLUrl`
