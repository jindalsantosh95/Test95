# Event Store Before Publish With Rollback

**Pattern group:** Workflow And Messaging

**Status:** Recommended

**Last reviewed:** 2026-06-01

**Owner:** EAPI Technical Leads

## Intent

Record an event in the EAPI event store before publishing it to Service Bus, then roll back or delete the event-store entry if publishing or downstream preparation fails.

This pattern provides a recoverable audit point for asynchronous workflows while avoiding orphaned event records when the publish step fails.

## Use When

- Publishing business-critical events.
- A workflow needs traceability or recovery across asynchronous boundaries.
- Follow-on processing depends on event identity and type.

## Do Not Use When

- The event is best-effort telemetry with no business consequence.
- The event store would contain sensitive data that should not be persisted.
- The workflow requires a different transactional or outbox mechanism approved for the service.

## Pattern Shape

1. Generate a stable event ID.
2. Create and enrich the CloudEvent.
3. Insert the event into the event store.
4. Publish the event to Service Bus.
5. On failure, log the error and roll back or delete the event-store entry.

## Implementation Guidance

- Use EAPI event-store command handlers.
- Persist only the data needed for recovery and traceability.
- Remove connection strings and operational settings before storing payloads.
- Roll back with the same event ID and type used for insert.

## Code References

- [FlightDataRequestEventCommandHandler.cs](../../VAA.EAPI.Orchastration/VAA.EAPI.Orchestration.Notifications/Application/Events/FlightDataRequestEventCommandHandler.cs:43): insert before publish and rollback on exception.
- [EventRequestInsertCommand.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Repository/EventStore/EventRequestInsertCommand.cs:31): event-store insert handler.
- [EventRequestRollBackCommand.cs](../../VAA.EAPI.Microservices/VAA.EAPI.Infrastructure/Repository/EventStore/EventRequestRollBackCommand.cs:1): event-store rollback support.

## Related Principles And Standards

- [Integration Principles](../principles/integration-principles.md)
- [Data Principles](../principles/data-principles.md)
- [Coding Standards](../standards/coding-standards.md)

## AI Agent Checklist

- Event is stored before publish when recoverability matters.
- Stored payload excludes secrets and unnecessary sensitive data.
- Publish failures trigger rollback or compensating cleanup.
- Logs include correlation and event ID.

## Change History

| Date | Change | Author |
| --- | --- | --- |
| 2026-06-01 | Initial draft. | Seb Harvey |
| 2026-06-01 | Approved. | EAPI Technical Leads |
