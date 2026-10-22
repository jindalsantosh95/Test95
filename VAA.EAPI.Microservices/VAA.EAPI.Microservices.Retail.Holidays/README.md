# Retail Holidays Microservice

This .NET 10 isolated-worker Azure Functions microservice is the EAPI facade for Retail Holidays capabilities backed by TravelBox through DMAZE.

Read the repository [Retail Holidays TravelBox DMAZE flow](../../docs/sequence-diagrams/vaa-eapi-retail-holidays-travelbox-dmaze.md) before changing or adding a DMAZE-backed capability. The [boundary decision](../../docs/decisions/2026-07-17-travelbox-dmaze-microservice-boundary.md) requires in-context TravelBox capabilities to be coalesced into this service rather than a separate TravelBox microservice.

## Existing Functions

| Function | Trigger and route | Purpose |
| --- | --- | --- |
| `RetrieveBooking` | `POST /retrieveBooking` | Retrieve and validate one holiday booking from DMAZE Cart Service. |
| `RetrieveBookingList` | `POST /retrieveBookingList` | Retrieve multiple holiday bookings in parallel. |
| `RetrieveDocumentList` | `POST /retrieveDocumentList` | Retrieve available booking documents. |
| `RetrievePendingDocumentList` | `POST /retrievePendingDocumentList` | Retrieve pending booking documents. |
| `RetrieveDocumentDownloadLink` | `GET /retrieveDocumentDownloadLink/{docId}` | Retrieve a document view/download link. |
| `RetrieveDocumentDownloadLinkList` | `POST /retrieveDocumentDownloadLinkList` | Retrieve multiple document links with bounded concurrency. |
| `AuthorisationTokens` | `POST /authorisationTokens` | Supply configured TravelBox claims during token issuance. |
| `Heartbeat` | `GET /heartbeat` | Return the registered health-check report. |
| `Warmup` | Azure Functions warmup trigger | Warm a new Function App instance. |

## Bruno Collection

Open `VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Holidays.bruno` in Bruno and select the `Local` environment to exercise every HTTP trigger against the local Functions host. The collection contains synthetic placeholder values only; replace them locally with suitable test data and do not commit credentials, tokens, subscription keys, customer data, or deployed environment URLs.

## Existing Application Services

- `IRetrieveBookingService` / `RetrieveBookingService` integrate with DMAZE Cart Service, validate the lead traveller, filter expired holidays, and map DMAZE responses to EAPI trip models.
- `IRetrieveDocumentService` / `RetrieveDocumentService` integrate with DMAZE Document Service for document lists, pending documents, and document view information.
- Both downstream services use `IOAuthTokenProvider` and `IHttpConnector` and send the established DMAZE authorization and TravelBox user headers.

## DMAZE Source Specifications

The two attached OpenAPI documents directly relevant to the existing implementation are:

| DMAZE service | Wiki page beneath `TravelBox™-APEX-(DMAZE)` | Attached specification |
| --- | --- | --- |
| Cart Service | `Services/Sales-&-Booking/Cart-Service` | `/.attachments/cart-service-0b42a612-a4b0-4974-9037-1ff289c930b7.json` |
| Document Service | `Services/Back-Office/Document-Service` | `/.attachments/document-service-d82752d7-d20a-4117-91fc-d882abff7ac3.json` |

The attachment paths are relative to the [DMAZE wiki](https://dev.azure.com/virginatlantic/Digital%20Distribution/_wiki/wikis/Digital-Distribution.wiki/32651/TravelBox%E2%84%A2-APEX-(DMAZE)). The full attachment catalogue is maintained in the repository [Retail Holidays TravelBox DMAZE flow](../../docs/sequence-diagrams/vaa-eapi-retail-holidays-travelbox-dmaze.md).

The project also contains `openai/swagger.json`, a checked-in Cart Service snapshot. Compare all snapshots with the target DMAZE environment's `/v3/api-docs` before implementation; do not assume their versions match.

## Configuration

| Key | Purpose |
| --- | --- |
| `Holidays:RetrieveBookingService:Url` | DMAZE Cart service base URL. |
| `Holidays:RetrieveDocumentService:Url` | DMAZE Document service base URL. |
| `Holidays:DocumentDownloadAllowedHosts` | Comma-separated exact host/port allowlist for DMAZE document downloads; requests fail closed when this setting is empty. |
| `Holidays:DocumentDownloadMaximumBytes` | Maximum permitted PDF download size in bytes; defaults to 5,242,880 bytes (5 MiB). |
| `Holidays:DocumentDownloadTimeoutSeconds` | Timeout for downloading a PDF; defaults to 30 seconds. |
| `Holidays:EntraScope` | Base scope used to acquire the DMAZE access token. |
| `Holidays:AuthorizationProvider` | DMAZE authorization-provider value; currently defaults to `AZURE`. |
| `Holidays:TbxUsername` | TravelBox username supplied by the token-issuance function. |
| `Holidays:TbxUserId` | TravelBox user identifier supplied by the token-issuance function. |
| `Holidays:AirlineCodesForAir4` | Airline codes used by the Holidays response mapping. |
| `EapiMsClientID` | Platform client identifier appended to `Holidays:EntraScope` when acquiring DMAZE access tokens. |

Do not commit values, credentials, tokens, client secrets, or environment URLs.

## Development Guidance

- Expose EAPI-owned capability contracts; do not return DMAZE wrappers or infrastructure DTOs.
- Keep new triggers thin and put validation, mapping, and orchestration in application handlers.
- Use capability-focused gateways backed by `IHttpConnector` for new DMAZE calls.
- Propagate cancellation and EAPI correlation identifiers.
- Map DMAZE success, warning, and error states explicitly.
- Do not log tokens, raw DMAZE responses, full bookings, passenger data, payment data, or document content.
- Existing anonymous triggers, `CancellationToken.None`, inline configuration-key declarations, and raw downstream-response logging are legacy context, not patterns for new work.
- Update OpenAPI, APIM, deployments, pipelines, health checks, telemetry, tests, and this README with each capability.

## Verification

```pwsh
dotnet test VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Holidays.Tests/VAA.EAPI.Microservices.Retail.Holidays.Tests.csproj -c Release --results-directory TestResults/<run-name> --logger "trx;LogFileName=<run-name>.trx" --collect:"XPlat Code Coverage"
scripts/analyse-test-run-results.ps1 -TrxPath TestResults/<run-name>/<run-name>.trx -SuiteName "Retail Holidays" -EnvironmentName "local"
scripts/verify.ps1 -ProjectPath VAA.EAPI.Microservices/VAA.EAPI.Microservices.Retail.Holidays/VAA.EAPI.Microservices.Retail.Holidays.csproj -SkipBuild
```
