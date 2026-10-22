# Shared services sign-off pack

The shared-services pack is a dedicated set of focused xUnit facts. Existing regression categories remain separate and continue to cover their broader scenarios.

## Included coverage

The `SharedServicesSignOff` category currently includes:

- free-seat selection on LHR-JFK;
- paid-seat selection on LHR-JFK;
- paid-seat cabin upgrade on LHR-JFK;
- Virgin and Delta seat-map coverage on LHR-JFK;
- special assistance retrieval and update;
- special meals retrieval and update; and
- logged-in-user trip details.

These scenarios exercise the GraphQL/mobile boundary used by FlightServicesMS, OrderDataMS, Retail.PaymentMS, and SeatMS. Paid-seat and paid-seat-upgrade sign-off facts carry both `SeatMS` and `Retail.PaymentMS` traits.

## Route prerequisite

The focused seat sign-off scope is LHR-JFK. Seat sign-off facts pass the route to the storage-table lookup, while the broader regression theories retain their existing unfiltered data selection. A test that falls back to another route must not be treated as sign-off evidence.

## GitHub Actions workflow

`.github/workflows/shared-services-sign-off.yml` is the single shared-services sign-off entry point. It supports manual DEV/SIT/UAT execution and can isolate evidence by shared service and consuming domain. The Trips pack uses `SharedServicesSignOff` facts tagged with the relevant shared service and `SharedServiceConsumer=Trips`.

```text
Category=SharedServicesSignOff&SharedService=SeatMS&SharedServiceConsumer=Trips
```

Configure the repository secret `SLACK_WEBHOOK_URL_SHARED_SERVICES` for the new Slack channel. The workflow publishes the result and links the run in Slack; no webhook credential is stored in this repository.

## Local discovery

From the repository root, use the owning project:

```powershell
dotnet test VAA.EAPI.Automation/VAA.EAPI.Automation.Trips/VAA.EAPI.Automation.Trips.csproj -c Release --no-build --filter "Category=SharedServicesSignOff&SharedService=SeatMS&SharedServiceConsumer=Trips" --list-tests
```
