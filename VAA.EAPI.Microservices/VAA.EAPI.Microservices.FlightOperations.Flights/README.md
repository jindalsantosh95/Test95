# Flight Operations Flights

Azure Functions isolated worker service for Flight Operations flight data.

In deployed environments, the release pipelines apply IP restrictions to the GraphQL and Flights Function Apps so inbound API traffic goes through API Management (APIM). Call these endpoints through their APIM routes; the restrictions block direct requests to the Function App URLs even when a publish profile is available. A publish profile is a deployment credential and must still be protected and revoked if exposed. Local requests can use the local Azure Functions host.

## Local Configuration

Configure settings through `local.settings.json` or environment variables. For local runs, put app settings under the `Values` object so the Azure Functions host promotes them to process environment variables:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FlightOperation:AircraftCategorySqlDbConnectionString": "<connection-string>",
    "FlightStatus:DeltaServiceLayerBaseUrl": "https://www.virginatlantic.com",
    "FlightStatus:RequestTimeoutSeconds": "20",
    "FlightStatus:EnrichmentDeadlineSeconds": "8",
    "FlightStatus:MaxRetries": "2",
    "FlightStatus:MaxConcurrentDownstreamRequests": "8"
  }
}
```

The check aircraft category endpoint reads the SQL connection string from `FlightOperation:AircraftCategorySqlDbConnectionString`.

When using the managed-identity token path, the connection string may contain server, database, encryption, trust, and timeout settings. Do not rely on SQL connection-string authentication for this path; shared infrastructure `SqlConnectionHelper` removes the `Authentication` keyword before assigning the managed identity token to `SqlConnection.AccessToken` with the Azure SQL scope `https://database.windows.net/.default`. Generic SQL query execution goes through infrastructure `ISqlQueryService`, while this project owns the SQL query, connection-string setting key, parameters, and response mapping.

Flight status reads `FlightStatus:DeltaServiceLayerBaseUrl` for the Delta Hosted API. When unset or invalid, the service uses `https://www.virginatlantic.com`. If the configured host is `www.virginatlantic.com`, the service normalizes the base URL to HTTPS. Request and enrichment deadlines are configurable; `FlightStatus:MaxRetries` is clamped to the GDD limit of two retries, and downstream concurrency is bounded by `FlightStatus:MaxConcurrentDownstreamRequests`.

## Check Aircraft Category

`POST /api/checkaircraftcategory`

```json
{
  "categoryIndex": 11,
  "tailNumbers": ["G-VTEA", "G-VWHO"]
}
```

The response returns each normalized tail number with `doesCategoryExist` set to `true` when a matching `ACCATEG` row exists for the requested category index.

## Flight Status

Flight status is a public read capability that any consumer can implement against through APIM, including the Operations GraphQL provider. The Azure Functions route prefix makes the endpoints available under `/api` when the host uses the default route prefix. The HTTP triggers use `AuthorizationLevel.Anonymous`; flight-status calls through APIM require an APIM subscription key, but no caller bearer token or Flight Operations app role. The GraphQL-to-Flights connection sends the APIM subscription key only.

The `VAA.EAPI.Microservices.FlightOperations.Flights.bruno/Flight_Status` folder contains requests for both current endpoints. Select the Local, DEV [APIM], or SIT [APIM] environment before sending them; the APIM environments require an `ocpkey` secret. The requests default to today's local date. Set `flightStatusDate` to `yyyy-MM-dd` in Bruno to query another date. Optional route filters are disabled by default; the flight-number request includes its required departure airport code.

`GET /api/flightstatus/routes/{departureAirportCode}/{arrivalAirportCode}/{scheduledDepartureDate}?timeRange=1,2,3&stops=0,1,2&first=true`

Route lookup validates that departure and arrival are distinct three-letter IATA airport codes and that the departure date is within the inclusive UTC window from yesterday through 331 days from today. Missing `timeRange`, `stops`, or `first` values default to `1,2,3`, `0,1,2`, and `true`. The service calls Delta `/flights/routes` once and maps only the implemented route fields: departure airport, arrival airport, scheduled departure date, routes, route legs, leg timings, aircraft, and status. Empty route results return `404`; malformed core route data or routes without usable legs return a controlled `502`.

`GET /api/flightstatus/flights/{flightNumber}/{departureDate}?departureAirportCode=LHR&selectedLegId=1`

Flight-number lookup validates an ASCII alphanumeric flight number of six characters or fewer, a required three-letter departure airport code, the optional selected leg id (up to 32 characters), and the same inclusive UTC date window. The service calls Delta `/flights/details`, selects the requested leg when present or falls back to the first returned leg, then concurrently requests `/stats`, `/flights/positions/`, and `/flights/amenitiesAndMeals` when the selected leg provides the required inputs. The arrival airport for enrichment comes from the selected leg, not a client argument. These populate performance, position, amenities, and meals in `activeLeg.detail`; route search does not call these enrichment endpoints. Failed or empty enrichment responses retain the flight details and produce the corresponding warning: `FLIGHT_STATUS_STATS_UNAVAILABLE`, `POSITION_UNAVAILABLE`, or `AMENITIES_MEALS_UNAVAILABLE`. If a usable leg lacks its scheduled departure time, the service skips enrichment and returns all three unavailable warnings.

Both endpoints return `400` for invalid input or a downstream request rejection, `404` when the downstream core lookup finds no flight status, and `200` for successful lookups. A failed downstream core call or unusable response returns `502`; throttling or known temporary unavailability returns `503`; a downstream timeout returns `504`. Error bodies contain stable flight status codes and generic messages, without downstream response details. If any optional enrichment fails or returns no data, flight details still return `200`; that enrichment field is null and its stable warning code is included as described above. APIM rejects requests without a valid subscription key or access to the flight-status route.

The flight status wrapper sends the required downstream service marker header on every Delta service layer request:

| Header | Value | Applied to |
| --- | --- | --- |
| `x-vaa-flight-status` | `1` | `/flights/routes`, `/flights/details`, `/stats`, `/flights/positions/`, and `/flights/amenitiesAndMeals` |

Delta's flight-status service layer is unauthenticated and provided as-is. This microservice does not control Delta's contract, availability, or changes to that service. Delta flight status requests do not send browser-compatible `Accept`, `Accept-Language`, `User-Agent`, or `Referer` headers, and they do not require bearer tokens or APIM subscription keys.

## Tests

Run the focused Flights test project from the microservices folder:

```powershell
dotnet test VAA.EAPI.Microservices.FlightOperations.Flights.Tests\VAA.EAPI.Microservices.FlightOperations.Flights.Tests.csproj -c Release
```
