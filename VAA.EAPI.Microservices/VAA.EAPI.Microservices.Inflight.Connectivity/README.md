# VAA.EAPI.Microservices.Inflight.Connectivity

Azure Functions microservice for Starlink inflight connectivity client registration APIs.

## Endpoints
- `POST /api/inflight/connectivity`: accepts `deviceId` and `sandboxId`, and forwards to Starlink clients endpoint with `expiry` set from request payload.
- `GET /api/heartbeat`: service heartbeat endpoint.

## Triggers
- `Warmup` trigger logs app warm-up events.

## Local development
1. Restore/build solution:
   - `dotnet restore VAA.EAPI.Microservices.sln`
   - `dotnet build VAA.EAPI.Microservices.sln -c Release`
2. Run Functions host from this project folder.

## Required configuration
- `StarlinkAuthenticationUrl`: OAuth token endpoint for Starlink.
- `StarlinkAPIClientId`: OAuth client id for token retrieval.
- `StarlinkClientSecret`: OAuth client secret (via managed identity/secret store).
- `StarlinkClientsEndpoint`: Starlink API endpoint for client registration requests.
