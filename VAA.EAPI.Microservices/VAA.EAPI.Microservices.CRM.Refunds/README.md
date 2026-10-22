# VAA.EAPI.Microservices.CRM.Refunds

Azure Functions isolated-worker microservice that supports customer refund eligibility checks, refund submission workflows, and file uploads for refund processing.

## Endpoints

- `POST /api/crm/refunds/status`
  - Input: array of `RefundItem` objects including `passengerName`, `flightNumber`, `ticketNumber`, `passengerRef`, `flightDate`, `couponNumber`, etc.
  - Action: calls D365 for each `RefundItem` and merges a status (`OK` / `NOTOK`) into each element.
- `POST /api/crm/refunds/submit`
  - Input: selected entries (must all be `OK`) + `firstName`, `lastName`, `emailAddress`, `reason`, `bookingReference`, `bookingDate`
  - Action: creates sample XML payload, sends to D365, publishes a sample Azure Service Bus notification message, and returns the D365 reference number.
- `GET /api/heartbeat`
  - Standard health endpoint.

## Triggers

- HTTP triggers in `Triggers/Http`.
- Warmup trigger in `HealthController`.

## Key configuration

- `D365BaseUrl`
- `D365RefundStatusPath`
- `D365RefundSubmissionPath`
- `RefundServiceBusConnectionString`
- `RefundServiceBusTopicName`
- `RefundServiceBusSubject`
- `RefundsHeartbeatSchedule`

## Local development

From `VAA.EAPI.Microservices/`:

1. `dotnet restore VAA.EAPI.Microservices.CRM.sln`
2. `dotnet build VAA.EAPI.Microservices.CRM.sln -c Release`
3. `dotnet test VAA.EAPI.Microservices.CRM.sln -c Release --filter Refunds`
